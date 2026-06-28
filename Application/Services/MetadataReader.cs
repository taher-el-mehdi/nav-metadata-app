using Microsoft.Data.SqlClient;
using NAVMetadata.Abstractions;
using NAVMetadata.Constants;
using NAVMetadata.Enums;
using NAVMetadata.Models;

namespace NAVMetadata.Services;

/// <summary>
/// Reads NAV metadata from SQL Server system tables.
/// </summary>
public sealed class MetadataReader : IMetadataReader
{
    private readonly IDatabaseConnectionService _connectionService;
    private readonly ILoggerService _logger;

    public MetadataReader(IDatabaseConnectionService connectionService, ILoggerService logger)
    {
        _connectionService = connectionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NavObject>> GetObjectsByTypeAsync(
        ObjectType objectType,
        CancellationToken cancellationToken = default)
    {
        var profile = RequireProfile();

        var results = new List<NavObject>();

        await using var connection = new SqlConnection(profile.BuildConnectionString());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(NavSqlQueries.ListObjectsByType, connection)
        {
            CommandTimeout = profile.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@Type", (int)objectType);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            results.Add(MapObject(objectType, reader));

        _logger.LogInfo($"Loaded {results.Count} {objectType} object(s)");
        return results;
    }

    /// <inheritdoc />
    public async Task<string?> GetObjectMetadataXmlAsync(
        ObjectType objectType,
        int objectId,
        CancellationToken cancellationToken = default)
    {
        var profile = RequireProfile();

        await using var connection = new SqlConnection(profile.BuildConnectionString());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(NavSqlQueries.DecompressObjectMetadata, connection)
        {
            CommandTimeout = profile.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@ObjectType", (int)objectType);
        command.Parameters.AddWithValue("@ObjectId", objectId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null or DBNull)
            return null;

        var xml = Convert.ToString(result);
        _logger.LogInfo($"Loaded metadata XML for {objectType} {objectId} ({xml?.Length ?? 0} chars)");
        return xml;
    }

    private ConnectionProfile RequireProfile() =>
        _connectionService.CurrentProfile
        ?? throw new InvalidOperationException("Connect to a database before reading metadata.");

    private static NavObject MapObject(ObjectType objectType, SqlDataReader reader)
    {
        var modifiedOrdinal = reader.GetOrdinal("Modified");
        var modified = !reader.IsDBNull(modifiedOrdinal) && Convert.ToBoolean(reader.GetValue(modifiedOrdinal));

        return new NavObject
        {
            Type = objectType,
            ObjectId = reader.GetInt32(reader.GetOrdinal("ID")),
            ObjectName = reader.GetString(reader.GetOrdinal("Name")),
            Modified = modified,
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("Date")) ? null : reader.GetDateTime(reader.GetOrdinal("Date")),
            VersionList = reader.IsDBNull(reader.GetOrdinal("Version List")) ? null : reader.GetString(reader.GetOrdinal("Version List"))
        };
    }
}
