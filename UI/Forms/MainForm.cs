using NAVMetadata.Abstractions;
using NAVMetadata.Constants;
using NAVMetadata.Enums;
using NAVMetadata.Helpers;
using NAVMetadata.Models;

namespace NAVMetadata.Forms;

/// <summary>
/// Main window — welcome screen until connected, then NAV Object Designer layout.
/// </summary>
public sealed class MainForm : Form
{
    private readonly INavigationService _navigation;
    private readonly IDatabaseConnectionService _database;
    private readonly IWorkspaceService _workspace;
    private readonly IMetadataReader _metadataReader;
    private readonly IConnectionSettingsService _connectionSettings;
    private readonly IUpdateCoordinator _updateCoordinator;
    private readonly ILoggerService _logger;

    private readonly Panel _welcomePanel;
    private readonly SplitContainer _explorerPanel;

    private readonly ListBox _typeList = new()
    {
        Dock = DockStyle.Fill,
        IntegralHeight = false,
        BorderStyle = BorderStyle.None,
        Font = AppFonts.UiList
    };

    private readonly ListView _objectList = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        GridLines = true,
        MultiSelect = false,
        HideSelection = false,
        Font = AppFonts.UiList
    };

    private readonly Button _viewButton = new() { Text = "View", Size = new Size(100, 32), Font = AppFonts.UiButton, Enabled = false };
    private readonly Button _exportButton = new() { Text = "Export", Size = new Size(100, 32), Font = AppFonts.UiButton, Enabled = false };

    private readonly TextBox _idFilterInput = new() { Width = 72, Font = AppFonts.UiNormal, PlaceholderText = "e.g. 18" };
    private readonly TextBox _nameFilterInput = new() { Width = 200, Font = AppFonts.UiNormal, PlaceholderText = "e.g. Customer" };
    private readonly Label _filterResultLabel = new() { AutoSize = true, Font = AppFonts.UiSmall, ForeColor = SystemColors.GrayText };

    private readonly Panel _filterPanel;
    private bool _filterPanelVisible;

    private readonly ToolStripStatusLabel _statusLabel = new()
    {
        Name = "ConnectionStatus",
        Spring = true,
        TextAlign = ContentAlignment.MiddleLeft,
        Font = AppFonts.UiSmall
    };

    private ToolStripMenuItem? _disconnectMenuItem;
    private ToolStripMenuItem? _refreshMenuItem;
    private ToolStripMenuItem? _filterMenuItem;
    private Button? _welcomeConnectButton;

    private sealed class TypeListEntry(ObjectType type, string displayText)
    {
        public ObjectType Type { get; } = type;
        public string DisplayText { get; } = displayText;

        public override string ToString() => DisplayText;
    }

    public MainForm(
        INavigationService navigation,
        IDatabaseConnectionService database,
        IWorkspaceService workspace,
        IMetadataReader metadataReader,
        IConnectionSettingsService connectionSettings,
        IUpdateCoordinator updateCoordinator,
        ILoggerService logger)
    {
        _navigation = navigation;
        _database = database;
        _workspace = workspace;
        _metadataReader = metadataReader;
        _connectionSettings = connectionSettings;
        _updateCoordinator = updateCoordinator;
        _logger = logger;

        Font = AppFonts.UiNormal;
        _filterPanel = CreateFilterPanel();
        _welcomePanel = CreateWelcomePanel();
        _explorerPanel = CreateExplorerPanel();

        BuildUi();
        ShowWelcomeState();
        Shown += OnFirstShown;
        _logger.LogInfo("Application started");
    }

    private void BuildUi()
    {
        Text = AppConstants.WindowTitle;
        Size = new Size(1200, 800);
        MinimumSize = new Size(800, 560);
        StartPosition = FormStartPosition.CenterScreen;
        AppBranding.ApplyTo(this);

        BuildMenuAndStatus(out var menu, out var status);

        var host = new Panel { Dock = DockStyle.Fill };
        host.Controls.Add(_explorerPanel);
        host.Controls.Add(_welcomePanel);

        Controls.Add(host);
        Controls.Add(status);
        Controls.Add(menu);

        UpdateConnectionUi();
    }

    private Panel CreateWelcomePanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            Padding = new Padding(24)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var content = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            Anchor = AnchorStyles.None
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var branding = BrandingHeader.CreateWelcomeBranding();
        branding.Anchor = AnchorStyles.None;
        content.Controls.Add(branding, 0, 0);

        _welcomeConnectButton = new Button
        {
            Text = "Connect to SQL Server",
            Font = AppFonts.UiButton,
            AutoSize = true,
            MinimumSize = new Size(220, 40),
            Padding = new Padding(16, 8, 16, 8),
            Margin = new Padding(0, 12, 0, 0),
            Anchor = AnchorStyles.None
        };
        _welcomeConnectButton.Click += async (_, _) => await ConnectAsync();
        content.Controls.Add(_welcomeConnectButton, 0, 1);

        layout.Controls.Add(content, 0, 0);
        panel.Controls.Add(layout);
        return panel;
    }

    private SplitContainer CreateExplorerPanel()
    {
        _typeList.DrawMode = DrawMode.OwnerDrawFixed;
        _typeList.ItemHeight = 36;
        _typeList.DrawItem += TypeList_DrawItem;

        _typeList.SelectedIndexChanged += (_, _) => PopulateObjectList();
        _objectList.SelectedIndexChanged += (_, _) => UpdateActionButtons();
        _objectList.DoubleClick += async (_, _) => await ViewSelectedMetadataAsync();
        _objectList.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                await ViewSelectedMetadataAsync();
            }
        };

        _idFilterInput.TextChanged += (_, _) => PopulateObjectList();
        _nameFilterInput.TextChanged += (_, _) => PopulateObjectList();
        _viewButton.Click += async (_, _) => await ViewSelectedMetadataAsync();
        _exportButton.Click += async (_, _) => await ExportSelectedMetadataAsync();

        _objectList.Columns.Add("Type", 72);
        _objectList.Columns.Add("ID", 64);
        _objectList.Columns.Add("Name", 240);
        _objectList.Columns.Add("Modified", 72);
        _objectList.Columns.Add("Version List", 200);
        _objectList.Columns.Add("Date / Time", 148);

        var typePanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4), BackColor = SystemColors.Control };
        typePanel.Controls.Add(_typeList);

        var buttonBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12, 8, 12, 8),
            BackColor = SystemColors.Control
        };
        buttonBar.Controls.Add(_exportButton);
        buttonBar.Controls.Add(_viewButton);

        var gridPanel = new Panel { Dock = DockStyle.Fill };
        gridPanel.Controls.Add(_objectList);
        gridPanel.Controls.Add(buttonBar);
        gridPanel.Controls.Add(_filterPanel);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 152,
            FixedPanel = FixedPanel.Panel1
        };
        split.Panel1.Controls.Add(typePanel);
        split.Panel2.Controls.Add(gridPanel);

        RefreshTypeList();
        return split;
    }

    private void TypeList_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= _typeList.Items.Count)
            return;

        var entry = (TypeListEntry)_typeList.Items[e.Index]!;
        var selected = (e.State & DrawItemState.Selected) != 0;

        var backColor = selected ? SystemColors.Highlight : SystemColors.Control;
        var textColor = selected ? SystemColors.HighlightText : SystemColors.ControlText;

        using (var background = new SolidBrush(backColor))
            e.Graphics.FillRectangle(background, e.Bounds);

        var textBounds = new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 4, e.Bounds.Width - 12, e.Bounds.Height - 10);
        TextRenderer.DrawText(
            e.Graphics,
            entry.DisplayText,
            AppFonts.UiList,
            textBounds,
            textColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        if ((e.State & DrawItemState.Focus) != 0)
            e.DrawFocusRectangle();
    }

    private Panel CreateFilterPanel()
    {
        var clearFilter = new Button
        {
            Text = "Clear",
            Font = AppFonts.UiButton,
            AutoSize = true,
            FlatStyle = FlatStyle.Standard,
            Padding = new Padding(8, 2, 8, 2)
        };
        clearFilter.Click += (_, _) =>
        {
            _idFilterInput.Clear();
            _nameFilterInput.Clear();
            _nameFilterInput.Focus();
        };

        var content = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(8, 4, 8, 6)
        };
        content.Controls.Add(new Label { Text = "ID", AutoSize = true, Font = AppFonts.UiBold, Padding = new Padding(0, 6, 4, 0) });
        content.Controls.Add(_idFilterInput);
        content.Controls.Add(new Label { Text = "Name", AutoSize = true, Font = AppFonts.UiBold, Padding = new Padding(16, 6, 4, 0) });
        content.Controls.Add(_nameFilterInput);
        content.Controls.Add(clearFilter);
        content.Controls.Add(_filterResultLabel);

        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 0,
            Visible = false,
            BackColor = Color.FromArgb(245, 245, 245)
        };
        panel.Controls.Add(content);
        panel.Paint += (_, e) =>
        {
            using var pen = new Pen(SystemColors.ControlDark, 1);
            e.Graphics.DrawLine(pen, 0, panel.Height - 1, panel.Width, panel.Height - 1);
        };

        return panel;
    }

    private async void OnFirstShown(object? sender, EventArgs e)
    {
        Shown -= OnFirstShown;

        if (await TryAutoConnectAsync())
        {
            _ = CheckForUpdatesOnStartupAsync();
            return;
        }

        // No saved connection or auto-connect failed — stay on welcome screen.
        _ = CheckForUpdatesOnStartupAsync();
    }

    private async Task CheckForUpdatesOnStartupAsync()
    {
        try
        {
            await _updateCoordinator.CheckOnStartupAsync(this);
        }
        catch (Exception ex)
        {
            _logger.LogError("Startup update check failed", ex);
        }
    }

    private async Task<bool> TryAutoConnectAsync()
    {
        var profile = await _connectionSettings.LoadAsync();
        if (profile is null)
            return false;

        UseWaitCursor = true;
        try
        {
            if (!await _navigation.ConnectWithProfileAsync(profile, showErrors: false))
                return false;

            ShowExplorerState();
            SelectFirstType();
            UpdateStatus();
            return true;
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void ShowWelcomeState()
    {
        _welcomePanel.Visible = true;
        _welcomePanel.BringToFront();
        _explorerPanel.Visible = false;
        UpdateConnectionUi();
        UpdateStatus();
    }

    private void ShowExplorerState()
    {
        _explorerPanel.Visible = true;
        _explorerPanel.BringToFront();
        _welcomePanel.Visible = false;
        UpdateConnectionUi();
    }

    private void UpdateConnectionUi()
    {
        var connected = _database.IsConnected;

        _disconnectMenuItem!.Enabled = connected;
        _refreshMenuItem!.Enabled = connected;
    }

    private void ToggleFilterPanel()
    {
        _filterPanelVisible = !_filterPanelVisible;
        _filterPanel.Visible = _filterPanelVisible;
        _filterPanel.Height = _filterPanelVisible ? 40 : 0;
        UpdateFilterMenuState();

        if (_filterPanelVisible)
            _nameFilterInput.Focus();
    }

    private void UpdateFilterMenuState()
    {
        if (_filterMenuItem is null)
            return;

        _filterMenuItem.Checked = _filterPanelVisible;
        _filterMenuItem.Text = HasActiveFilter() ? "&Filter  •" : "&Filter";
    }

    private void BuildMenuAndStatus(out MenuStrip menu, out StatusStrip status)
    {
        menu = new MenuStrip { Font = AppFonts.UiNormal };
        var file = new ToolStripMenuItem("&File");
        file.DropDownItems.Add("&Connect...", null, async (_, _) => await ConnectAsync());
        _disconnectMenuItem = new ToolStripMenuItem("&Disconnect", null, async (_, _) => await DisconnectAsync());
        file.DropDownItems.Add(_disconnectMenuItem);
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add("E&xit", null, (_, _) => Close());
        menu.Items.Add(file);

        var view = new ToolStripMenuItem("&View");
        _filterMenuItem = new ToolStripMenuItem("&Filter", null, (_, _) => ToggleFilterPanel());
        view.DropDownItems.Add(_filterMenuItem);
        view.DropDownItems.Add(new ToolStripSeparator());
        _refreshMenuItem = new ToolStripMenuItem("&Refresh", null, async (_, _) => await RefreshAsync()) { ShortcutKeys = Keys.F5 };
        view.DropDownItems.Add(_refreshMenuItem);
        view.DropDownItems.Add(new ToolStripSeparator());
        view.DropDownItems.Add("&View Metadata", null, async (_, _) => await ViewSelectedMetadataAsync());
        view.DropDownItems.Add("&Export Metadata...", null, async (_, _) => await ExportSelectedMetadataAsync());
        menu.Items.Add(view);

        var help = new ToolStripMenuItem("&Help");
        help.DropDownItems.Add("Check for &Updates...", null, async (_, _) => await CheckForUpdatesManuallyAsync());
        help.DropDownItems.Add(new ToolStripSeparator());
        help.DropDownItems.Add("Report an &Issue...", null, (_, _) => ExternalLinks.OpenReportIssue());
        help.DropDownItems.Add(new ToolStripSeparator());
        help.DropDownItems.Add("&About...", null, (_, _) => ShowAboutDialog());
        menu.Items.Add(help);
        MainMenuStrip = menu;

        status = new StatusStrip { Font = AppFonts.UiSmall };
        status.Items.Add(_statusLabel);
    }

    private async Task ConnectAsync()
    {
        if (await _navigation.ShowConnectionDialogAsync(this))
        {
            ShowExplorerState();
            SelectFirstType();
            UpdateStatus();
        }
    }

    private async Task DisconnectAsync()
    {
        await _database.DisconnectAsync();
        _workspace.Clear();
        ClearObjectList();
        ShowWelcomeState();
    }

    private async Task RefreshAsync()
    {
        if (!_database.IsConnected)
        {
            _navigation.ShowMessage("Connect to a database first.", "Not Connected", MessageType.Warning);
            return;
        }

        try
        {
            await _workspace.RefreshAsync();
            RefreshTypeList();
            PopulateObjectList();
            UpdateStatus();
        }
        catch (Exception ex)
        {
            _logger.LogError("Refresh failed", ex);
            _navigation.ShowMessage(ex.Message, "Refresh Failed", MessageType.Error);
        }
    }

    private void SelectFirstType()
    {
        RefreshTypeList();

        if (_typeList.Items.Count > 0 && _typeList.SelectedIndex < 0)
            _typeList.SelectedIndex = 0;
        else
            PopulateObjectList();
    }

    private void RefreshTypeList()
    {
        var previous = GetSelectedObjectType();

        _typeList.BeginUpdate();
        _typeList.Items.Clear();

        foreach (var type in NavObjectCatalog.BrowsableTypes)
        {
            var count = _workspace.HasData && _workspace.ObjectsByType.TryGetValue(type, out var objects)
                ? objects.Count
                : 0;
            _typeList.Items.Add(new TypeListEntry(type, $"{type} ({count})"));
        }

        _typeList.EndUpdate();

        if (previous is null)
            return;

        for (var i = 0; i < _typeList.Items.Count; i++)
        {
            if (_typeList.Items[i] is TypeListEntry entry && entry.Type == previous)
            {
                _typeList.SelectedIndex = i;
                return;
            }
        }
    }

    private ObjectType? GetSelectedObjectType() =>
        _typeList.SelectedItem is TypeListEntry entry ? entry.Type : null;

    private void PopulateObjectList()
    {
        _objectList.BeginUpdate();
        _objectList.Items.Clear();
        UpdateActionButtons();

        if (GetSelectedObjectType() is not { } type || !_workspace.HasData)
        {
            _objectList.EndUpdate();
            return;
        }

        if (!_workspace.ObjectsByType.TryGetValue(type, out var objects))
        {
            _objectList.EndUpdate();
            return;
        }

        var filtered = ObjectListFilter.Apply(objects, _idFilterInput.Text, _nameFilterInput.Text).ToList();
        _filterResultLabel.Text = HasActiveFilter()
            ? $"{filtered.Count} of {objects.Count} objects"
            : string.Empty;
        UpdateFilterMenuState();

        foreach (var obj in filtered)
        {
            var item = new ListViewItem(obj.Type.ToString());
            item.SubItems.Add(obj.ObjectId.ToString());
            item.SubItems.Add(obj.ObjectName);
            item.SubItems.Add(obj.Modified ? "Yes" : "");
            item.SubItems.Add(obj.VersionList ?? "");
            item.SubItems.Add(obj.ModifiedDate?.ToString("g") ?? "");
            item.Tag = obj;
            _objectList.Items.Add(item);
        }

        _objectList.EndUpdate();
    }

    private void ClearObjectList()
    {
        _typeList.ClearSelected();
        _objectList.Items.Clear();
        _idFilterInput.Clear();
        _nameFilterInput.Clear();
        _filterResultLabel.Text = string.Empty;

        if (_filterPanelVisible)
        {
            _filterPanelVisible = false;
            _filterPanel.Visible = false;
            _filterPanel.Height = 0;
        }

        RefreshTypeList();
        UpdateFilterMenuState();
        UpdateActionButtons();
    }

    private bool HasActiveFilter() =>
        !string.IsNullOrWhiteSpace(_idFilterInput.Text) ||
        !string.IsNullOrWhiteSpace(_nameFilterInput.Text);

    private void UpdateActionButtons()
    {
        var hasSelection = _objectList.SelectedItems.Count > 0;
        _viewButton.Enabled = hasSelection;
        _exportButton.Enabled = hasSelection;
    }

    private NavObject? GetSelectedObject() =>
        _objectList.SelectedItems.Count > 0 && _objectList.SelectedItems[0].Tag is NavObject obj
            ? obj
            : null;

    private async Task<string?> LoadMetadataForSelectedAsync(NavObject obj)
    {
        try
        {
            UseWaitCursor = true;
            return await _metadataReader.GetObjectMetadataXmlAsync(obj.Type, obj.ObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load metadata for {obj.DisplayName}", ex);
            _navigation.ShowMessage(ex.Message, "Metadata Load Failed", MessageType.Error);
            return null;
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task ViewSelectedMetadataAsync()
    {
        if (GetSelectedObject() is not { } obj)
            return;

        var xml = await LoadMetadataForSelectedAsync(obj);
        if (xml is null)
        {
            _navigation.ShowMessage("No metadata found for this object.", "View Metadata", MessageType.Warning);
            return;
        }

        UseWaitCursor = true;
        try
        {
            var formatted = await Task.Run(() => XmlFormatter.TryFormat(xml));
            MetadataViewForm.ShowDialog(obj, formatted, this);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task ExportSelectedMetadataAsync()
    {
        if (GetSelectedObject() is not { } obj)
            return;

        var xml = await LoadMetadataForSelectedAsync(obj);
        if (xml is null)
        {
            _navigation.ShowMessage("No metadata found for this object.", "Export Metadata", MessageType.Warning);
            return;
        }

        var safeName = string.Concat(obj.ObjectName.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        using var dialog = new SaveFileDialog
        {
            Title = "Export Metadata",
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = $"{obj.Type}{obj.ObjectId}-{safeName}.xml",
            DefaultExt = "xml"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            await File.WriteAllTextAsync(dialog.FileName, XmlFormatter.TryFormat(xml));
            _statusLabel.Text = $"Exported metadata to {dialog.FileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to export metadata for {obj.DisplayName}", ex);
            _navigation.ShowMessage(ex.Message, "Export Failed", MessageType.Error);
        }
    }

    private void UpdateStatus()
    {
        _statusLabel.Text = _database.IsConnected
            ? $"Connected — {_database.CurrentProfile?.DisplayLabel} — {_workspace.TotalCount} object(s)"
            : "Not connected — choose a NAV database to begin";
    }

    private async Task CheckForUpdatesManuallyAsync()
    {
        try
        {
            UseWaitCursor = true;
            await _updateCoordinator.CheckManuallyAsync(this);
        }
        catch (Exception ex)
        {
            _logger.LogError("Manual update check failed", ex);
            MessageBox.Show(this, "Unable to check for updates.", AppConstants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void ShowAboutDialog()
    {
        using var about = new AboutDialog(_updateCoordinator);
        about.ShowDialog(this);
    }
}
