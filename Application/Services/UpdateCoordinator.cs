using NAVMetadata.Abstractions;
using NAVMetadata.Constants;
using NAVMetadata.Enums;
using NAVMetadata.Forms;
using NAVMetadata.Models;

namespace NAVMetadata.Services;

/// <summary>
/// Coordinates update checks, persisted preferences, and user prompts.
/// </summary>
public sealed class UpdateCoordinator : IUpdateCoordinator
{
    private readonly IUpdateService _updateService;
    private readonly IUpdateSettingsService _updateSettingsService;
    private readonly ILoggerService _logger;

    public UpdateCoordinator(
        IUpdateService updateService,
        IUpdateSettingsService updateSettingsService,
        ILoggerService logger)
    {
        _updateService = updateService;
        _updateSettingsService = updateSettingsService;
        _logger = logger;
    }

    public Task CheckOnStartupAsync(IWin32Window owner, CancellationToken cancellationToken = default) =>
        RunCheckAsync(owner, forcePrompt: false, cancellationToken);

    public Task CheckManuallyAsync(IWin32Window owner, CancellationToken cancellationToken = default) =>
        RunCheckAsync(owner, forcePrompt: true, cancellationToken);

    private async Task RunCheckAsync(IWin32Window owner, bool forcePrompt, CancellationToken cancellationToken)
    {
        var settings = await _updateSettingsService.LoadAsync(cancellationToken);
        var result = await _updateService.CheckForUpdatesAsync(cancellationToken);

        settings.LastCheckUtc = DateTime.UtcNow;
        await _updateSettingsService.SaveAsync(settings, cancellationToken);

        if (!result.CheckSucceeded)
        {
            if (forcePrompt)
                ShowCheckFailed(owner, result.ErrorMessage ?? "Unable to check for updates.");

            return;
        }

        if (!result.IsUpdateAvailable || result.LatestRelease is null)
        {
            if (forcePrompt)
                ShowUpToDate(owner, result.CurrentVersion);

            return;
        }

        if (!forcePrompt && ShouldSuppressPrompt(settings, result.LatestRelease.Version))
        {
            _logger.LogInfo($"Suppressing update prompt for version {result.LatestRelease.Version}");
            return;
        }

        var choice = UpdateDialog.ShowDialog(owner, result);
        await ApplyPromptChoiceAsync(choice, result.LatestRelease, settings, cancellationToken);
    }

    private static bool ShouldSuppressPrompt(UpdateSettings settings, string latestVersion)
    {
        if (string.Equals(settings.SkippedVersion, latestVersion, StringComparison.OrdinalIgnoreCase))
            return true;

        return settings.RemindAfterUtc is { } remindAfter && remindAfter > DateTime.UtcNow;
    }

    private async Task ApplyPromptChoiceAsync(
        UpdatePromptResult choice,
        ReleaseInfo release,
        UpdateSettings settings,
        CancellationToken cancellationToken)
    {
        switch (choice)
        {
            case UpdatePromptResult.UpdateNow:
                try
                {
                    await _updateService.OpenReleasePageAsync(release, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to open release page", ex);
                    MessageBox.Show(
                        $"Could not open the download page.\n\nVisit:\n{release.ReleasePageUrl}",
                        AppConstants.AppName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                break;

            case UpdatePromptResult.RemindMeLater:
                settings.RemindAfterUtc = DateTime.UtcNow.Add(AppConstants.UpdateRemindLaterInterval);
                await _updateSettingsService.SaveAsync(settings, cancellationToken);
                _logger.LogInfo($"Update reminder deferred until {settings.RemindAfterUtc:u}");
                break;

            case UpdatePromptResult.SkipThisVersion:
                settings.SkippedVersion = release.Version;
                settings.RemindAfterUtc = null;
                await _updateSettingsService.SaveAsync(settings, cancellationToken);
                _logger.LogInfo($"Skipped update version {release.Version}");
                break;
        }
    }

    private static void ShowUpToDate(IWin32Window owner, string currentVersion)
    {
        MessageBox.Show(
            owner,
            $"{AppConstants.AppName} is up to date.\n\nYou are running version {currentVersion}.",
            "No Updates Available",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private static void ShowCheckFailed(IWin32Window owner, string message)
    {
        MessageBox.Show(
            owner,
            message,
            "Update Check Failed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }
}
