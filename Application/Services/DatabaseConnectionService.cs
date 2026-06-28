using Microsoft.Data.SqlClient;
using NAVMetadata.Abstractions;
using NAVMetadata.Constants;
using NAVMetadata.Models;

namespace NAVMetadata.Services;

/// <summary>
/// Manages the active SQL connection profile.
/// </summary>
public sealed class DatabaseConnectionService : IDatabaseConnectionService
{
    private readonly ILoggerService _logger;
    private ConnectionProfile? _currentProfile;

    public DatabaseConnectionService(ILoggerService logger) => _logger = logger;

    /// <inheritdoc />
    public bool IsConnected => _currentProfile is not null;

    /// <inheritdoc />
    public ConnectionProfile? CurrentProfile => _currentProfile;

    /// <inheritdoc />
    public async Task<bool> ConnectAsync(ConnectionProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (!await TestConnectionAsync(profile, cancellationToken))
            return false;

        _currentProfile = profile;
        _logger.LogInfo($"Connected to {profile.DisplayLabel}");
        return true;
    }

    /// <inheritdoc />
    public Task DisconnectAsync()
    {
        if (_currentProfile is not null)
            _logger.LogInfo($"Disconnected from {_currentProfile.DisplayLabel}");

        _currentProfile = null;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync(ConnectionProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        try
        {
            await using var connection = new SqlConnection(profile.BuildConnectionString());
            await connection.OpenAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Connection test failed for {profile.DisplayLabel}", ex);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetAvailableDatabasesAsync(
        ConnectionProfile profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var databases = new List<string>();

        try
        {
            await using var connection = new SqlConnection(profile.BuildConnectionString("master"));
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(NavSqlQueries.ListUserDatabases, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
                databases.Add(reader.GetString(0));

            _logger.LogInfo($"Found {databases.Count} database(s) on {profile.ServerName}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to list databases on {profile.ServerName}", ex);
            throw;
        }

        return databases;
    }

    /// <inheritdoc />
    public async Task<bool> IsNavDatabaseAsync(ConnectionProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        try
        {
            await using var connection = new SqlConnection(profile.BuildConnectionString());
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(NavSqlQueries.HasObjectTable, connection);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is not null and not DBNull && Convert.ToInt32(result) == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to verify NAV database {profile.DisplayLabel}", ex);
            return false;
        }
    }
}
