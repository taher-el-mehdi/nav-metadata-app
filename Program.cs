using Microsoft.Extensions.DependencyInjection;
using NAVMetadata.Composition;
using NAVMetadata.Constants;
using NAVMetadata.Forms;
using NAVMetadata.Helpers;
using Serilog;

namespace NAVMetadata;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetDefaultFont(AppFonts.UiNormal);

        try
        {
            var services = AppBootstrap.Build();
            var mainForm = services.GetRequiredService<MainForm>();
            Application.Run(mainForm);

            if (services is IDisposable disposable)
                disposable.Dispose();
        }
        catch (Exception ex)
        {
            ExternalLinks.ShowErrorWithReportOption($"Fatal error: {ex.Message}", AppConstants.AppName);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
