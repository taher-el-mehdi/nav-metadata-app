using System.Text.Json;
using NAVMetadata.Abstractions;
using NAVMetadata.Constants;
using NAVMetadata.Models;

namespace NAVMetadata.Services;

/// <summary>
/// Saves the last connection to a JSON file under LocalAppData.
/// SQL passwords are encrypted with Windows DPAPI.
/// </summary>
public sealed class ConnectionSettingsService : IConnectionSettingsService
{
    private readonly ILoggerService _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ConnectionSettingsService(ILoggerService logger) => _logger = logger;

    /// <inheritdoc />
    public async Task<ConnectionProfile?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(AppConstants.ConnectionSettingsPath))
            return null;

        try
        {
            await using var stream = File.OpenRead(AppConstants.ConnectionSettingsPath);
            var dto = await JsonSerializer.DeserializeAsync<ConnectionSettingsDto>(stream, cancellationToken: cancellationToken);

            if (dto is null || string.IsNullOrWhiteSpace(dto.ServerName) || string.IsNullOrWhiteSpace(dto.DatabaseName))
                return null;

            _logger.LogInfo($"Loaded saved connection for {dto.ServerName}/{dto.DatabaseName}");
            return dto.ToProfile();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load saved connection settings", ex);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(ConnectionProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        try
        {
            Directory.CreateDirectory(AppConstants.AppDataFolder);

            await using var stream = File.Create(AppConstants.ConnectionSettingsPath);
            await JsonSerializer.SerializeAsync(stream, ConnectionSettingsDto.From(profile), JsonOptions, cancellationToken);

            _logger.LogInfo($"Saved connection settings for {profile.DisplayLabel}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save connection settings", ex);
        }
    }
}
