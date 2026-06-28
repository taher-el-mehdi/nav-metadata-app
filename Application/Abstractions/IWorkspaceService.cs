using NAVMetadata.Enums;
using NAVMetadata.Models;

namespace NAVMetadata.Abstractions;

/// <summary>
/// In-memory cache of NAV objects loaded from the database. The UI reads from here.
/// </summary>
public interface IWorkspaceService
{
    /// <summary>True after a successful refresh with at least one object type loaded.</summary>
    bool HasData { get; }

    /// <summary>Objects grouped by type. Populated by <see cref="RefreshAsync"/>.</summary>
    IReadOnlyDictionary<ObjectType, IReadOnlyList<NavObject>> ObjectsByType { get; }

    /// <summary>Total number of objects across all types.</summary>
    int TotalCount { get; }

    /// <summary>Reloads all browsable object types from the database.</summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);

    /// <summary>Clears cached objects (e.g. on disconnect).</summary>
    void Clear();
}
