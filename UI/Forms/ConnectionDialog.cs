using NAVMetadata.Abstractions;
using NAVMetadata.Constants;
using NAVMetadata.Enums;
using NAVMetadata.Helpers;
using NAVMetadata.Models;

namespace NAVMetadata.Forms;

/// <summary>
/// SQL Server Management Studio-style connection dialog.
/// User sets server and authentication, then databases load automatically.
/// </summary>
public sealed class ConnectionDialog : Form
{
    private readonly IDatabaseConnectionService _databaseService;
    private readonly IConnectionSettingsService _connectionSettings;
    private ConnectionProfile? _selectedProfile;

    private readonly ComboBox _serverInput = new()
    {
        Dock = DockStyle.Fill,
        Font = AppFonts.UiNormal,
        DropDownStyle = ComboBoxStyle.DropDown,
        Margin = new Padding(0, 10, 0, 10)
    };

    private readonly ComboBox _authCombo = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
        Font = AppFonts.UiNormal,
        Margin = new Padding(0, 10, 0, 10)
    };

    private readonly TextBox _usernameInput = new()
    {
        Dock = DockStyle.Fill,
        Font = AppFonts.UiNormal,
        ReadOnly = true,
        BackColor = SystemColors.Control,
        Margin = new Padding(0, 10, 0, 10)
    };

    private readonly TextBox _passwordInput = new()
    {
        Dock = DockStyle.Fill,
        UseSystemPasswordChar = true,
        Enabled = false,
        Font = AppFonts.UiNormal,
        Margin = new Padding(0, 10, 0, 10)
    };

    private readonly ComboBox _databaseCombo = new()
    {
        Dock = DockStyle.Fill,
        Font = AppFonts.UiNormal,
        DropDownStyle = ComboBoxStyle.DropDown,
        AutoCompleteMode = AutoCompleteMode.SuggestAppend,
        AutoCompleteSource = AutoCompleteSource.ListItems,
        MinimumSize = new Size(0, 28),
        Margin = new Padding(0, 10, 0, 10)
    };

    private readonly Button _connectButton;
    private readonly Label _databaseStatusLabel;

    private bool _isLoading;

    public ConnectionDialog(IDatabaseConnectionService databaseService, IConnectionSettingsService connectionSettings)
    {
        _databaseService = databaseService;
        _connectionSettings = connectionSettings;
        Font = AppFonts.UiNormal;

        _databaseStatusLabel = new Label
        {
            Text = string.Empty,
            Font = AppFonts.UiSmall,
            ForeColor = SystemColors.GrayText,
            Dock = DockStyle.Fill,
            AutoSize = true,
            MaximumSize = new Size(900, 0),
            Padding = new Padding(0, 2, 0, 0)
        };

        _connectButton = new Button
        {
            Text = "Connect",
            Font = AppFonts.UiButton,
            AutoSize = true,
            MinimumSize = new Size(96, 32),
            Enabled = false
        };
        _connectButton.Click += async (_, _) => await OnConnectAsync();

        BuildUi();
        ApplyWindowsAuthState();
        Shown += async (_, _) =>
        {
            await ApplySavedSettingsAsync();
            if (_databaseCombo.Items.Count == 0 && CanLoadDatabases())
                await TryLoadDatabasesAsync();
        };
    }

    /// <summary>Profile chosen by the user when the dialog closes with OK.</summary>
    public ConnectionProfile? SelectedProfile => _selectedProfile;

    private void BuildUi()
    {
        Text = "Connect to Server";
        Size = new Size(640, 520);
        MinimumSize = new Size(540, 480);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = SystemColors.Control;
        AppBranding.ApplyTo(this);

        _serverInput.Items.AddRange(["(local)", ".", "localhost"]);
        _serverInput.Text = "(local)";

        _authCombo.Items.AddRange(["Windows Authentication", "SQL Server Authentication"]);
        _authCombo.SelectedIndex = 0;
        _serverInput.Leave += async (_, _) => await TryLoadDatabasesAsync();
        _passwordInput.Leave += async (_, _) => await TryLoadDatabasesAsync();
        _usernameInput.Leave += async (_, _) => await TryLoadDatabasesAsync();
        _authCombo.SelectedIndexChanged += async (_, _) =>
        {
            ApplyWindowsAuthState();
            ClearDatabaseList();
            await TryLoadDatabasesAsync();
        };

        _databaseCombo.TextChanged += (_, _) => UpdateConnectButtonState();
        _databaseCombo.SelectedIndexChanged += (_, _) => UpdateConnectButtonState();

        var header = CreateHeader();
        var connectionPanel = CreateConnectionPanel();
        var buttonBar = CreateButtonBar();

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 8, 16, 12),
            AutoScroll = true
        };
        body.Controls.Add(buttonBar);
        body.Controls.Add(connectionPanel);
        body.Controls.Add(header);

        Controls.Add(body);
        AcceptButton = _connectButton;
        CancelButton = buttonBar.Controls.OfType<Button>().First(b => b.DialogResult == DialogResult.Cancel);
    }

    private Panel CreateHeader()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            BackColor = Color.White,
            Padding = new Padding(12, 10, 12, 8)
        };

        var logo = new PictureBox
        {
            Image = AppBranding.LogoImage,
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(36, 36),
            Dock = DockStyle.Left
        };

        var title = new Label
        {
            Text = "Connect to SQL Server",
            Font = new Font(AppFonts.UiFamily, 14f, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = Color.FromArgb(0, 70, 130),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };

        var separator = new Panel { Dock = DockStyle.Bottom, Height = 3 };
        separator.Paint += (_, e) =>
        {
            using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                separator.ClientRectangle,
                Color.FromArgb(255, 140, 0),
                Color.FromArgb(180, 180, 180),
                System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(brush, separator.ClientRectangle);
        };

        header.Controls.Add(title);
        header.Controls.Add(logo);
        header.Controls.Add(separator);
        return header;
    }

    private TableLayoutPanel CreateConnectionPanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(8, 24, 8, 12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 6; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(MakeFieldLabel("Server name:"), 0, 0);
        layout.Controls.Add(_serverInput, 1, 0);
        layout.Controls.Add(MakeFieldLabel("Authentication:"), 0, 1);
        layout.Controls.Add(_authCombo, 1, 1);
        layout.Controls.Add(MakeFieldLabel("User name:"), 0, 2);
        layout.Controls.Add(_usernameInput, 1, 2);
        layout.Controls.Add(MakeFieldLabel("Password:"), 0, 3);
        layout.Controls.Add(_passwordInput, 1, 3);
        layout.Controls.Add(MakeFieldLabel("Database:"), 0, 4);
        layout.Controls.Add(_databaseCombo, 1, 4);
        layout.Controls.Add(_databaseStatusLabel, 1, 5);

        return layout;
    }

    private void UpdateConnectButtonState()
    {
        _connectButton.Enabled = GetSelectedDatabaseName() is not null;
    }

    private string? GetSelectedDatabaseName()
    {
        var databaseName = _databaseCombo.Text.Trim();
        return string.IsNullOrWhiteSpace(databaseName) ? null : databaseName;
    }

    private FlowLayoutPanel CreateButtonBar()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12, 10, 12, 10),
            WrapContents = false
        };

        var cancel = new Button
        {
            Text = "Cancel",
            Font = AppFonts.UiButton,
            DialogResult = DialogResult.Cancel,
            AutoSize = true,
            MinimumSize = new Size(96, 32)
        };

        bar.Controls.Add(cancel);
        bar.Controls.Add(_connectButton);
        return bar;
    }

    private static Label MakeFieldLabel(string text) => new()
    {
        Text = text,
        Font = AppFonts.UiNormal,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        TextAlign = ContentAlignment.MiddleLeft,
        Margin = new Padding(0, 10, 20, 10)
    };

    private void ApplyWindowsAuthState()
    {
        var sqlAuth = _authCombo.SelectedIndex == 1;

        if (sqlAuth)
        {
            _usernameInput.ReadOnly = false;
            _usernameInput.BackColor = SystemColors.Window;
            _usernameInput.Enabled = true;
            if (_usernameInput.Text.Contains('\\'))
                _usernameInput.Clear();
            _passwordInput.Enabled = true;
        }
        else
        {
            _usernameInput.ReadOnly = true;
            _usernameInput.BackColor = SystemColors.Control;
            _usernameInput.Enabled = true;
            _usernameInput.Text = $@"{Environment.UserDomainName}\{Environment.UserName}";
            _passwordInput.Enabled = false;
            _passwordInput.Clear();
        }
    }

    private void ClearDatabaseList()
    {
        _databaseCombo.Items.Clear();
        _databaseCombo.Text = string.Empty;
        _connectButton.Enabled = false;
        _databaseStatusLabel.Text = string.Empty;
    }

    private bool CanLoadDatabases()
    {
        if (string.IsNullOrWhiteSpace(_serverInput.Text))
            return false;

        if (_authCombo.SelectedIndex == 1 && string.IsNullOrWhiteSpace(_usernameInput.Text))
            return false;

        return true;
    }

    private async Task TryLoadDatabasesAsync()
    {
        if (CanLoadDatabases())
            await LoadDatabasesAsync();
    }

    private ConnectionProfile BuildProfile(string databaseName) => new()
    {
        ServerName = _serverInput.Text.Trim(),
        DatabaseName = databaseName,
        AuthenticationType = _authCombo.SelectedIndex == 0 ? AuthenticationType.Windows : AuthenticationType.SqlServer,
        Username = _authCombo.SelectedIndex == 1 ? _usernameInput.Text.Trim() : null,
        Password = _passwordInput.Text
    };

    private async Task LoadDatabasesAsync()
    {
        if (_isLoading)
            return;

        if (string.IsNullOrWhiteSpace(_serverInput.Text))
        {
            MessageBox.Show("Enter a server name first.", "Connect to Server", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _serverInput.Focus();
            return;
        }

        if (_authCombo.SelectedIndex == 1 && string.IsNullOrWhiteSpace(_usernameInput.Text))
        {
            MessageBox.Show("Enter a user name for SQL Server Authentication.", "Connect to Server", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _usernameInput.Focus();
            return;
        }

        _isLoading = true;
        _databaseCombo.Items.Clear();
        _databaseCombo.Text = string.Empty;
        _connectButton.Enabled = false;
        _databaseStatusLabel.Text = "Loading…";
        Cursor = Cursors.WaitCursor;

        try
        {
            var databases = await _databaseService.GetAvailableDatabasesAsync(BuildProfile("master"));

            foreach (var db in databases)
                _databaseCombo.Items.Add(db);

            if (_databaseCombo.Items.Count == 0)
            {
                _databaseStatusLabel.Text = "No databases found. Check server name and authentication.";
                MessageBox.Show(
                    "No databases found on this server.",
                    "Connect to Server",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            _databaseStatusLabel.Text = string.Empty;
            _databaseCombo.SelectedIndex = 0;
            UpdateConnectButtonState();
        }
        catch (Exception ex)
        {
            _databaseStatusLabel.Text = "Could not load databases.";
            ExternalLinks.ShowErrorWithReportOption(ex.Message, "Could not connect to server");
        }
        finally
        {
            _isLoading = false;
            Cursor = Cursors.Default;
        }
    }

    private async Task ApplySavedSettingsAsync()
    {
        var saved = await _connectionSettings.LoadAsync();
        if (saved is null)
            return;

        _serverInput.Text = saved.ServerName;
        _authCombo.SelectedIndex = saved.AuthenticationType == AuthenticationType.Windows ? 0 : 1;
        ApplyWindowsAuthState();

        if (saved.AuthenticationType == AuthenticationType.SqlServer)
        {
            _usernameInput.Text = saved.Username ?? "";
            _passwordInput.Text = saved.Password ?? "";
        }

        await LoadDatabasesAsync();

        for (var i = 0; i < _databaseCombo.Items.Count; i++)
        {
            if (string.Equals(_databaseCombo.Items[i]?.ToString(), saved.DatabaseName, StringComparison.OrdinalIgnoreCase))
            {
                _databaseCombo.SelectedIndex = i;
                break;
            }
        }

        if (_databaseCombo.SelectedIndex < 0)
            _databaseCombo.Text = saved.DatabaseName;

        UpdateConnectButtonState();
    }

    private async Task OnConnectAsync()
    {
        if (GetSelectedDatabaseName() is not { } databaseName)
        {
            MessageBox.Show("Enter or select a database.", "Connect to Server", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _databaseCombo.Focus();
            return;
        }

        var profile = BuildProfile(databaseName);
        Cursor = Cursors.WaitCursor;
        Enabled = false;

        try
        {
            if (!await _databaseService.TestConnectionAsync(profile))
            {
                MessageBox.Show(
                    "Connection test failed. Check your settings and try again.",
                    "Connect to Server",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (!await _databaseService.IsNavDatabaseAsync(profile))
            {
                MessageBox.Show(AppMessages.NotNavDatabase, "Not a NAV Database", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _selectedProfile = profile;
            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            Cursor = Cursors.Default;
            Enabled = true;
        }
    }
}
