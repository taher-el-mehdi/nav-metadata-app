namespace NAVMetadata.Models;

/// <summary>Information about a published application release.</summary>
public sealed class ReleaseInfo
{
    public required string Version { get; init; }

    public required string ReleaseNotes { get; init; }

    /// <summary>Browser URL for the GitHub Release page.</summary>
    public required string ReleasePageUrl { get; init; }

    /// <summary>Direct asset download URL when available (reserved for future auto-install).</summary>
    public string? DownloadUrl { get; init; }
}
