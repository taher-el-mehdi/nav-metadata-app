using NAVMetadata.Models;

namespace NAVMetadata.Abstractions;

/// <summary>Checks GitHub Releases for application updates.</summary>
public interface IUpdateService
{
    string CurrentVersion { get; }

    Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    /// <summary>Opens the release page in the default browser. Future auto-download can extend this contract.</summary>
    Task OpenReleasePageAsync(ReleaseInfo release, CancellationToken cancellationToken = default);
}
