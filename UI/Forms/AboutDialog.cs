using NAVMetadata.Abstractions;
using NAVMetadata.Constants;
using NAVMetadata.Helpers;

namespace NAVMetadata.Forms;

/// <summary>
/// About dialog with version information and a manual update check action.
/// </summary>
public sealed class AboutDialog : Form
{
    private readonly IUpdateCoordinator _updateCoordinator;
    private readonly Button _checkUpdatesButton;

    public AboutDialog(IUpdateCoordinator updateCoordinator)
    {
        _updateCoordinator = updateCoordinator;

        Font = AppFonts.UiNormal;
        Text = $"About {AppConstants.AppName}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(460, 320);
        BackColor = Color.White;
        AppBranding.ApplyTo(this);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(28, 24, 28, 20)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(CreateBrandingBlock(), 0, 0);
        layout.Controls.Add(CreateDescriptionPanel(), 0, 1);
        layout.Controls.Add(CreateButtonBar(out _checkUpdatesButton), 0, 2);

        Controls.Add(layout);
    }

    private static Control CreateBrandingBlock()
    {
        var logo = new PictureBox
        {
            Image = AppBranding.LogoImage,
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(48, 48),
            Margin = new Padding(0, 0, 12, 0)
        };

        var title = new Label
        {
            Text = AppConstants.AppName,
            Font = AppFonts.UiTitle,
            ForeColor = Color.FromArgb(0, 70, 130),
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 0)
        };

        var version = new Label
        {
            Text = $"Version {AppConstants.AppVersion}",
            Font = AppFonts.UiSmall,
            ForeColor = SystemColors.GrayText,
            AutoSize = true,
            Margin = new Padding(10, 14, 0, 0)
        };

        var titleRow = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };
        titleRow.Controls.Add(logo);
        titleRow.Controls.Add(title);
        titleRow.Controls.Add(version);

        var accent = new Panel
        {
            Height = 2,
            Width = 380,
            BackColor = Color.FromArgb(255, 140, 0),
            Margin = new Padding(0, 0, 0, 12)
        };

        var stack = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        stack.Controls.Add(titleRow);
        stack.Controls.Add(accent);

        return stack;
    }

    private static Control CreateDescriptionPanel()
    {
        var description = new Label
        {
            Text =
                $"""
                {AppConstants.AppTagline}

                {AppConstants.AppShortTagline}

                Report bugs and request features on GitHub.
                """,
            Font = AppFonts.UiSubtitle,
            ForeColor = Color.FromArgb(64, 64, 64),
            AutoSize = true,
            MaximumSize = new Size(400, 0),
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 8)
        };

        var copyright = new Label
        {
            Text = AppConstants.CopyrightNotice,
            Font = AppFonts.UiSmall,
            ForeColor = SystemColors.GrayText,
            AutoSize = true,
            Dock = DockStyle.Top
        };

        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(copyright);
        panel.Controls.Add(description);

        return panel;
    }

    private Control CreateButtonBar(out Button checkUpdatesButton)
    {
        var okButton = new Button
        {
            Text = "OK",
            Font = AppFonts.UiButton,
            DialogResult = DialogResult.OK,
            AutoSize = true,
            MinimumSize = new Size(88, 34),
            Padding = new Padding(16, 4, 16, 4),
            Margin = new Padding(8, 0, 0, 0)
        };
        AcceptButton = okButton;
        CancelButton = okButton;

        checkUpdatesButton = new Button
        {
            Text = "Check for Updates",
            Font = AppFonts.UiButton,
            AutoSize = true,
            MinimumSize = new Size(130, 34),
            Padding = new Padding(12, 4, 12, 4),
            Margin = new Padding(8, 0, 0, 0)
        };
        checkUpdatesButton.Click += async (_, _) => await CheckForUpdatesAsync();

        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = true
        };
        bar.Controls.Add(okButton);
        bar.Controls.Add(checkUpdatesButton);

        return bar;
    }

    private async Task CheckForUpdatesAsync()
    {
        _checkUpdatesButton.Enabled = false;
        UseWaitCursor = true;

        try
        {
            await _updateCoordinator.CheckManuallyAsync(this);
        }
        finally
        {
            UseWaitCursor = false;
            _checkUpdatesButton.Enabled = true;
        }
    }
}
