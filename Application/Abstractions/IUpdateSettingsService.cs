using NAVMetadata.Models;

namespace NAVMetadata.Abstractions;

/// <summary>Persists update-check preferences (skipped version, remind-later date).</summary>
public interface IUpdateSettingsService
{
    Task<UpdateSettings> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(UpdateSettings settings, CancellationToken cancellationToken = default);
}
