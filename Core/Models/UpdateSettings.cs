namespace NAVMetadata.Models;

/// <summary>User preferences for update checks and prompts.</summary>
public sealed class UpdateSettings
{
    public string? SkippedVersion { get; set; }

    public DateTime? LastCheckUtc { get; set; }

    public DateTime? RemindAfterUtc { get; set; }
}
