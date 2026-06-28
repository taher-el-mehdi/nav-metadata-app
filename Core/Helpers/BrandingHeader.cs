using NAVMetadata.Constants;

namespace NAVMetadata.Helpers;

/// <summary>Centered branding block for the welcome / login screen.</summary>
public static class BrandingHeader
{
    public static Control CreateWelcomeBranding(int logoSize = 48)
    {
        var logo = new PictureBox
        {
            Image = AppBranding.LogoImage,
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(logoSize, logoSize),
            Margin = new Padding(0, 0, 12, 0)
        };

        var title = new Label
        {
            Text = AppConstants.AppName,
            Font = AppFonts.UiTitle,
            ForeColor = Color.FromArgb(0, 70, 130),
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 0)
        };

        var version = new Label
        {
            Text = $"v{AppConstants.AppVersion}",
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
            Margin = new Padding(0)
        };
        titleRow.Controls.Add(logo);
        titleRow.Controls.Add(title);
        titleRow.Controls.Add(version);

        var accent = new Panel
        {
            Height = 2,
            AutoSize = false,
            Width = titleRow.PreferredSize.Width,
            BackColor = Color.FromArgb(255, 140, 0),
            Margin = new Padding(0, 10, 0, 16),
            Anchor = AnchorStyles.None
        };

        var stack = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            Anchor = AnchorStyles.None
        };
        stack.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.Controls.Add(titleRow, 0, 0);
        stack.Controls.Add(accent, 0, 1);

        stack.Layout += (_, _) =>
        {
            accent.Width = titleRow.Width;
        };

        return stack;
    }
}
