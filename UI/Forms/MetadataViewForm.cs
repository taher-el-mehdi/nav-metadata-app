using NAVMetadata.Constants;
using NAVMetadata.Helpers;
using NAVMetadata.Models;

namespace NAVMetadata.Forms;

/// <summary>
/// Read-only metadata editor — opens like NAV Object Designer "Design" view.
/// </summary>
public sealed class MetadataViewForm : Form
{
    private readonly RichTextBox _editor = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        WordWrap = false,
        BorderStyle = BorderStyle.None,
        Font = AppFonts.Mono,
        BackColor = Color.FromArgb(255, 255, 254),
        ForeColor = Color.FromArgb(30, 30, 30),
        ScrollBars = RichTextBoxScrollBars.Both
    };

    private MetadataViewForm(NavObject obj, string xml)
    {
        Text = $"{obj.Type} {obj.ObjectId} - {obj.ObjectName}";
        Font = AppFonts.UiNormal;
        Size = new Size(900, 650);
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(480, 320);
        AppBranding.ApplyTo(this);

        _editor.Text = xml;
        Controls.Add(_editor);
        Shown += async (_, _) => await TryApplyHighlightAsync(xml);
    }

    /// <summary>Opens a modal metadata viewer for the given object.</summary>
    public static void ShowDialog(NavObject obj, string xml, IWin32Window? owner)
    {
        using var form = new MetadataViewForm(obj, xml);
        form.ShowDialog(owner);
    }

    private async Task TryApplyHighlightAsync(string xml)
    {
        try
        {
            await XmlSyntaxHighlighter.ApplyAsync(_editor, xml);
        }
        catch
        {
            // Plain text is already visible.
        }
    }
}
