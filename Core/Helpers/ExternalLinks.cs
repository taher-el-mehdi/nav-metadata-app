using System.Diagnostics;
using NAVMetadata.Constants;

namespace NAVMetadata.Helpers;

/// <summary>Opens external URLs in the default browser.</summary>
public static class ExternalLinks
{
    public static void OpenReportIssue()
    {
        try
        {
            Process.Start(new ProcessStartInfo(AppConstants.ReportIssueUrl) { UseShellExecute = true });
        }
        catch (Exception)
        {
            MessageBox.Show(
                $"Could not open the browser. Visit:\n{AppConstants.ReportIssueUrl}",
                AppConstants.AppName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    public static void ShowErrorWithReportOption(string message, string title)
    {
        var result = MessageBox.Show(
            $"{message}\n\n{AppMessages.ReportIssuePrompt}",
            title,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button2);

        if (result == DialogResult.Yes)
            OpenReportIssue();
    }
}
