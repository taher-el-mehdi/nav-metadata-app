using System.Text.Json;
using NAVMetadata.Abstractions;
using NAVMetadata.Constants;
using NAVMetadata.Models;

namespace NAVMetadata.Services;

/// <summary>
/// Persists update preferences to a JSON file under LocalAppData.
/// </summary>
public sealed class UpdateSettingsService : IUpdateSettingsService
{
    private readonly ILoggerService _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public UpdateSettingsService(ILoggerService logger) => _logger = logger;

    public async Task<UpdateSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(AppConstants.UpdateSettingsPath))
            return new UpdateSettings();

        try
        {
            await using var stream = File.OpenRead(AppConstants.UpdateSettingsPath);
            var dto = await JsonSerializer.DeserializeAsync<UpdateSettingsDto>(stream, cancellationToken: cancellationToken);
            return dto?.ToSettings() ?? new UpdateSettings();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load update settings", ex);
            return new UpdateSettings();
        }
    }

    public async Task SaveAsync(UpdateSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            Directory.CreateDirectory(AppConstants.AppDataFolder);

            await using var stream = File.Create(AppConstants.UpdateSettingsPath);
            await JsonSerializer.SerializeAsync(stream, UpdateSettingsDto.From(settings), JsonOptions, cancellationToken);

            _logger.LogInfo("Saved update settings");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save update settings", ex);
        }
    }
}
