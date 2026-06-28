namespace NAVMetadata.Constants;

/// <summary>
/// Application-wide constants. Change values here — not scattered across the codebase.
/// </summary>
public static class AppConstants
{
    public const string AppName = "NAV Metadata";
    public const string AppTagline = "The open-source toolkit for Microsoft Dynamics NAV metadata.";
    public const string AppShortTagline = "Explore. Compare. Export.";

    public static string AppVersion => Helpers.AppVersion.Current;

    public static string WindowTitle => $"{AppName} v{AppVersion}";

    public const string CopyrightNotice = "Copyright Taher el mehdi 2026";

    /// <summary>GitHub issue tracker for bug reports and feature requests.</summary>
    public const string ReportIssueUrl = "https://github.com/taher-el-mehdi/nav-metadata/issues";

    public const string GitHubOwner = "taher-el-mehdi";
    public const string GitHubRepo = "nav-metadata";

    public static string GitHubLatestReleaseApiUrl =>
        $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";

    public static string GitHubReleasesPageUrl =>
        $"https://github.com/{GitHubOwner}/{GitHubRepo}/releases";

    public static string WebsiteUrl => "https://navmetadata.com";

    /// <summary>How long "Remind Me Later" suppresses automatic update prompts.</summary>
    public static readonly TimeSpan UpdateRemindLaterInterval = TimeSpan.FromDays(1);

    /// <summary>NAV system table that lists all application objects.</summary>
    public const string ObjectTable = "Object";

    /// <summary>NAV system table that stores compiled metadata blobs.</summary>
    public const string ObjectMetadataTable = "Object Metadata";

    public const int DefaultConnectionTimeoutSeconds = 15;
    public const int DefaultCommandTimeoutSeconds = 30;

    /// <summary>Folder for logs and saved settings.</summary>
    public static string AppDataFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NAVMetadata");

    /// <summary>Last successful connection (JSON).</summary>
    public static string ConnectionSettingsPath => Path.Combine(AppDataFolder, "connection.json");

    /// <summary>Update-check preferences (JSON).</summary>
    public static string UpdateSettingsPath => Path.Combine(AppDataFolder, "update-settings.json");
}
