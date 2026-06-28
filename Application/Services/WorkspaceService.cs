using NAVMetadata.Abstractions;
using NAVMetadata.Constants;
using NAVMetadata.Enums;
using NAVMetadata.Models;

namespace NAVMetadata.Services;

/// <summary>
/// In-memory snapshot of NAV objects loaded from the database.
/// </summary>
public sealed class WorkspaceService : IWorkspaceService
{
    private readonly IMetadataReader _metadataReader;
    private readonly ILoggerService _logger;
    private Dictionary<ObjectType, IReadOnlyList<NavObject>> _objectsByType = new();

    public WorkspaceService(IMetadataReader metadataReader, ILoggerService logger)
    {
        _metadataReader = metadataReader;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool HasData => _objectsByType.Count > 0;

    /// <inheritdoc />
    public IReadOnlyDictionary<ObjectType, IReadOnlyList<NavObject>> ObjectsByType => _objectsByType;

    /// <inheritdoc />
    public int TotalCount => _objectsByType.Values.Sum(list => list.Count);

    /// <inheritdoc />
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInfo("Refreshing workspace metadata...");
        var snapshot = new Dictionary<ObjectType, IReadOnlyList<NavObject>>();

        foreach (var type in NavObjectCatalog.BrowsableTypes)
            snapshot[type] = await _metadataReader.GetObjectsByTypeAsync(type, cancellationToken);

        _objectsByType = snapshot;
        _logger.LogInfo($"Workspace loaded {TotalCount} object(s)");
    }

    /// <inheritdoc />
    public void Clear()
    {
        _objectsByType = new Dictionary<ObjectType, IReadOnlyList<NavObject>>();
        _logger.LogInfo("Workspace cleared");
    }
}
