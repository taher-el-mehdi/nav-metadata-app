using NAVMetadata.Models;

namespace NAVMetadata.Abstractions;

/// <summary>
/// Manages the active SQL Server connection to a NAV database.
/// </summary>
public interface IDatabaseConnectionService
{
    /// <summary>True when a connection profile is active.</summary>
    bool IsConnected { get; }

    /// <summary>The current connection profile, or null when disconnected.</summary>
    ConnectionProfile? CurrentProfile { get; }

    /// <summary>Tests and stores the given profile as the active connection.</summary>
    Task<bool> ConnectAsync(ConnectionProfile profile, CancellationToken cancellationToken = default);

    /// <summary>Clears the active connection.</summary>
    Task DisconnectAsync();

    /// <summary>Opens a short-lived connection to verify the profile works.</summary>
    Task<bool> TestConnectionAsync(ConnectionProfile profile, CancellationToken cancellationToken = default);

    /// <summary>Lists user databases available on the server (connects to <c>master</c>).</summary>
    Task<IReadOnlyList<string>> GetAvailableDatabasesAsync(ConnectionProfile profile, CancellationToken cancellationToken = default);

    /// <summary>True when the database contains the NAV <c>[Object]</c> system table.</summary>
    Task<bool> IsNavDatabaseAsync(ConnectionProfile profile, CancellationToken cancellationToken = default);
}
