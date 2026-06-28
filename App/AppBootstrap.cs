using Microsoft.Extensions.DependencyInjection;
using NAVMetadata.Abstractions;
using NAVMetadata.Forms;
using NAVMetadata.Services;
using Serilog;

namespace NAVMetadata.Composition;

/// <summary>
/// Application startup — logging configuration and dependency injection.
/// Register new services in <c>App/AppBootstrap.cs</c> when adding features.
/// </summary>
public static class AppBootstrap
{
    /// <summary>Builds the DI container for the application lifetime.</summary>
    public static IServiceProvider Build()
    {
        ConfigureLogging();

        var services = new ServiceCollection();
        RegisterServices(services);
        return services.BuildServiceProvider();
    }

    private static void ConfigureLogging()
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NAVMetadata",
            "logs",
            "app-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("Logging initialized at {LogPath}", logPath);
    }

    private static void RegisterServices(IServiceCollection services)
    {
        // Services — singleton (one instance for the app lifetime)
        services.AddSingleton<IConnectionSettingsService, ConnectionSettingsService>();
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<IDatabaseConnectionService, DatabaseConnectionService>();
        services.AddSingleton<IMetadataReader, MetadataReader>();
        services.AddSingleton<IWorkspaceService, WorkspaceService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IUpdateSettingsService, UpdateSettingsService>();
        services.AddSingleton<IUpdateService, GitHubUpdateService>();
        services.AddSingleton<IUpdateCoordinator, UpdateCoordinator>();

        // Forms
        services.AddSingleton<MainForm>();
        services.AddTransient<ConnectionDialog>();
    }
}
