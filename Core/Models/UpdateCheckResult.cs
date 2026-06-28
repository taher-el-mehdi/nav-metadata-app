namespace NAVMetadata.Models;

/// <summary>Outcome of checking GitHub for a newer release.</summary>
public sealed class UpdateCheckResult
{
    public required string CurrentVersion { get; init; }

    public ReleaseInfo? LatestRelease { get; init; }

    public bool IsUpdateAvailable { get; init; }

    public bool CheckSucceeded { get; init; }

    public string? ErrorMessage { get; init; }

    public static UpdateCheckResult Failed(string currentVersion, string errorMessage) => new()
    {
        CurrentVersion = currentVersion,
        CheckSucceeded = false,
        IsUpdateAvailable = false,
        ErrorMessage = errorMessage
    };

    public static UpdateCheckResult UpToDate(string currentVersion, ReleaseInfo? latestRelease = null) => new()
    {
        CurrentVersion = currentVersion,
        LatestRelease = latestRelease,
        CheckSucceeded = true,
        IsUpdateAvailable = false
    };

    public static UpdateCheckResult UpdateAvailable(string currentVersion, ReleaseInfo latestRelease) => new()
    {
        CurrentVersion = currentVersion,
        LatestRelease = latestRelease,
        CheckSucceeded = true,
        IsUpdateAvailable = true
    };
}
