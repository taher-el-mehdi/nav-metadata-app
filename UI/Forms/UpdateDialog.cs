using NAVMetadata.Constants;
using NAVMetadata.Enums;
using NAVMetadata.Helpers;
using NAVMetadata.Models;

namespace NAVMetadata.Forms;

/// <summary>
/// Modern dialog shown when a newer GitHub release is available.
/// </summary>
public sealed class UpdateDialog : Form
{
    private UpdateDialog(UpdateCheckResult result)
    {
        ArgumentNullException.ThrowIfNull(result.LatestRelease);

        Font = AppFonts.UiNormal;
        Text = "Update Available";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(520, 460);
        BackColor = Color.White;
        AppBranding.ApplyTo(this);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(24, 20, 24, 16)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(CreateHeader(), 0, 0);
        layout.Controls.Add(CreateVersionPanel(result), 0, 1);
        layout.Controls.Add(CreateReleaseNotesPanel(result.LatestRelease.ReleaseNotes), 0, 2);
        layout.Controls.Add(CreateButtonBar(), 0, 3);

        Controls.Add(layout);
    }

    public static UpdatePromptResult ShowDialog(IWin32Window? owner, UpdateCheckResult result)
    {
        using var dialog = new UpdateDialog(result);
        dialog.ShowDialog(owner);
        return dialog.PromptResult;
    }

    private UpdatePromptResult PromptResult { get; set; } = UpdatePromptResult.RemindMeLater;

    private Control CreateHeader()
    {
        var logo = new PictureBox
        {
            Image = AppBranding.LogoImage,
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(36, 36),
            Margin = new Padding(0, 0, 12, 0)
        };

        var title = new Label
        {
            Text = "A new version is available",
            Font = new Font(AppFonts.UiFamily, 16f, FontStyle.Regular),
            ForeColor = Color.FromArgb(0, 70, 130),
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 0)
        };

        var row = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };
        row.Controls.Add(logo);
        row.Controls.Add(title);

        var accent = new Panel
        {
            Height = 2,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(255, 140, 0),
            Margin = new Padding(0, 0, 0, 12)
        };

        var stack = new Panel { AutoSize = true, Dock = DockStyle.Top };
        stack.Controls.Add(accent);
        stack.Controls.Add(row);
        accent.Dock = DockStyle.Bottom;

        return stack;
    }

    private static Control CreateVersionPanel(UpdateCheckResult result)
    {
        var panel = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 2,
            Margin = new Padding(0, 0, 0, 12),
            BackColor = Color.FromArgb(248, 249, 251),
            Padding = new Padding(12, 10, 12, 10)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddVersionRow(panel, 0, "Current version", result.CurrentVersion);
        AddVersionRow(panel, 1, "Latest version", result.LatestRelease!.Version, highlight: true);

        return panel;
    }

    private static void AddVersionRow(TableLayoutPanel panel, int row, string label, string value, bool highlight = false)
    {
        panel.Controls.Add(new Label
        {
            Text = label,
            Font = AppFonts.UiBold,
            ForeColor = SystemColors.GrayText,
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, row);

        panel.Controls.Add(new Label
        {
            Text = value,
            Font = AppFonts.UiNormal,
            ForeColor = highlight ? Color.FromArgb(0, 120, 60) : SystemColors.ControlText,
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 1, row);
    }

    private static Control CreateReleaseNotesPanel(string releaseNotes)
    {
        var notes = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Font = AppFonts.UiSmall,
            BackColor = Color.FromArgb(252, 252, 252),
            BorderStyle = BorderStyle.FixedSingle,
            Text = releaseNotes
        };

        var group = new GroupBox
        {
            Text = "Release notes",
            Font = AppFonts.UiBold,
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 18, 10, 10),
            Margin = new Padding(0, 0, 0, 12)
        };
        group.Controls.Add(notes);

        return group;
    }

    private Control CreateButtonBar()
    {
        var updateNow = CreatePrimaryButton("Update Now", (_, _) =>
        {
            PromptResult = UpdatePromptResult.UpdateNow;
            DialogResult = DialogResult.OK;
            Close();
        });
        AcceptButton = updateNow;

        var remindLater = CreateSecondaryButton("Remind Me Later", (_, _) =>
        {
            PromptResult = UpdatePromptResult.RemindMeLater;
            DialogResult = DialogResult.Cancel;
            Close();
        });

        var skipVersion = CreateSecondaryButton("Skip This Version", (_, _) =>
        {
            PromptResult = UpdatePromptResult.SkipThisVersion;
            DialogResult = DialogResult.Ignore;
            Close();
        });

        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(0, 4, 0, 0)
        };
        bar.Controls.Add(updateNow);
        bar.Controls.Add(remindLater);
        bar.Controls.Add(skipVersion);

        return bar;
    }

    private Button CreatePrimaryButton(string text, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            Font = AppFonts.UiButton,
            AutoSize = true,
            MinimumSize = new Size(110, 34),
            Padding = new Padding(12, 4, 12, 4),
            Margin = new Padding(8, 0, 0, 0),
            FlatStyle = FlatStyle.Standard
        };
        button.Click += onClick;
        return button;
    }

    private Button CreateSecondaryButton(string text, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            Font = AppFonts.UiButton,
            AutoSize = true,
            MinimumSize = new Size(110, 34),
            Padding = new Padding(12, 4, 12, 4),
            Margin = new Padding(8, 0, 0, 0),
            FlatStyle = FlatStyle.Standard
        };
        button.Click += onClick;
        return button;
    }
}
