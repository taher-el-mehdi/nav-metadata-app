using NAVMetadata.Enums;
using NAVMetadata.Models;

namespace NAVMetadata.Abstractions;

/// <summary>
/// Reads NAV object lists and decompressed metadata XML from SQL Server.
/// All database queries for metadata belong here.
/// </summary>
public interface IMetadataReader
{
    /// <summary>Loads every object of the given type from the <c>[Object]</c> table.</summary>
    Task<IReadOnlyList<NavObject>> GetObjectsByTypeAsync(ObjectType objectType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decompresses and returns the metadata XML for one object from <c>[Object Metadata]</c>.
    /// Returns null when no row exists.
    /// </summary>
    Task<string?> GetObjectMetadataXmlAsync(ObjectType objectType, int objectId, CancellationToken cancellationToken = default);
}
