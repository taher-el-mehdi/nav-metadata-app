using NAVMetadata.Models;

namespace NAVMetadata.Abstractions;

/// <summary>
/// Persists the last successful SQL connection for auto-login on next startup.
/// </summary>
public interface IConnectionSettingsService
{
    /// <summary>Loads the saved connection, or null when none exists.</summary>
    Task<ConnectionProfile?> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>Saves the connection after a successful login.</summary>
    Task SaveAsync(ConnectionProfile profile, CancellationToken cancellationToken = default);
}
