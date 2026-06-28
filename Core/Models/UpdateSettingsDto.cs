namespace NAVMetadata.Models;

/// <summary>JSON-serializable update preferences stored on disk.</summary>
internal sealed class UpdateSettingsDto
{
    public string? SkippedVersion { get; set; }

    public DateTime? LastCheckUtc { get; set; }

    public DateTime? RemindAfterUtc { get; set; }

    public static UpdateSettingsDto From(UpdateSettings settings) => new()
    {
        SkippedVersion = settings.SkippedVersion,
        LastCheckUtc = settings.LastCheckUtc,
        RemindAfterUtc = settings.RemindAfterUtc
    };

    public UpdateSettings ToSettings() => new()
    {
        SkippedVersion = SkippedVersion,
        LastCheckUtc = LastCheckUtc,
        RemindAfterUtc = RemindAfterUtc
    };
}
