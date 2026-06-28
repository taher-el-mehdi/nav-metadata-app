using NAVMetadata.Enums;

namespace NAVMetadata.Models;

/// <summary>
/// One row from the NAV <c>[Object]</c> table.
/// </summary>
public sealed class NavObject
{
    public ObjectType Type { get; init; }
    public int ObjectId { get; init; }
    public required string ObjectName { get; init; }
    public bool Modified { get; init; }
    public DateTime? ModifiedDate { get; init; }
    public string? VersionList { get; init; }

    /// <summary>Human-readable label for list views and logs.</summary>
    public string DisplayName => $"{Type} {ObjectId} - {ObjectName}";
}
