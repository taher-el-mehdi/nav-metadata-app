using Microsoft.Extensions.DependencyInjection;
using NAVMetadata.Abstractions;
using NAVMetadata.Constants;
using NAVMetadata.Forms;
using NAVMetadata.Helpers;
using NAVMetadata.Models;

namespace NAVMetadata.Services;

/// <summary>
/// Handles connect flow: show dialog → connect → load workspace.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDatabaseConnectionService _databaseService;
    private readonly IWorkspaceService _workspaceService;
    private readonly IConnectionSettingsService _connectionSettings;
    private readonly ILoggerService _logger;

    public NavigationService(
        IServiceProvider serviceProvider,
        IDatabaseConnectionService databaseService,
        IWorkspaceService workspaceService,
        IConnectionSettingsService connectionSettings,
        ILoggerService logger)
    {
        _serviceProvider = serviceProvider;
        _databaseService = databaseService;
        _workspaceService = workspaceService;
        _connectionSettings = connectionSettings;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> ShowConnectionDialogAsync(object? owner = null)
    {
        var dialog = _serviceProvider.GetRequiredService<ConnectionDialog>();

        var result = owner is Form parent
            ? dialog.ShowDialog(parent)
            : dialog.ShowDialog();

        if (result != DialogResult.OK || dialog.SelectedProfile is null)
            return false;

        return await ConnectWithProfileAsync(dialog.SelectedProfile);
    }

    /// <inheritdoc />
    public async Task<bool> ConnectWithProfileAsync(
        ConnectionProfile profile,
        CancellationToken cancellationToken = default,
        bool showErrors = true)
    {
        if (!await _databaseService.ConnectAsync(profile, cancellationToken))
        {
            if (showErrors)
            {
                ShowMessage(
                    "Could not connect. Check server name, database, and credentials.",
                    "Connection Failed",
                    MessageType.Error);
            }
            else
            {
                _logger.LogInfo("Auto-connect failed: could not connect to server.");
            }

            return false;
        }

        if (!await _databaseService.IsNavDatabaseAsync(profile, cancellationToken))
        {
            await _databaseService.DisconnectAsync();
            if (showErrors)
                ShowMessage(AppMessages.NotNavDatabase, "Not a NAV Database", MessageType.Warning);
            else
                _logger.LogInfo("Auto-connect failed: database is not a NAV database.");

            return false;
        }

        try
        {
            await _workspaceService.RefreshAsync(cancellationToken);
            await _connectionSettings.SaveAsync(profile, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Connected but failed to load metadata", ex);
            await _databaseService.DisconnectAsync();
            _workspaceService.Clear();
            if (showErrors)
                ShowMessage(ex.Message, "Metadata Load Failed", MessageType.Error);

            return false;
        }
    }

    /// <inheritdoc />
    public void ShowMessage(string message, string title, MessageType messageType = MessageType.Info)
    {
        if (messageType == MessageType.Error)
        {
            ExternalLinks.ShowErrorWithReportOption(message, title);
            return;
        }

        var icon = messageType switch
        {
            MessageType.Warning => MessageBoxIcon.Warning,
            _ => MessageBoxIcon.Information
        };

        MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
    }
}
