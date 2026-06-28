namespace NAVMetadata.Abstractions;

using NAVMetadata.Models;

/// <summary>
/// Orchestrates modal dialogs and user notifications.
/// Keeps connection + workspace loading flow out of form code.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Shows the connection dialog, connects, and loads workspace metadata.
    /// Returns true when the user connected successfully.
    /// </summary>
    Task<bool> ShowConnectionDialogAsync(object? owner = null);

    /// <summary>
    /// Connects with a known profile, validates NAV database, and loads the workspace.
    /// Saves settings on success.
    /// </summary>
    /// <param name="showErrors">When false, failures are logged only (used for silent auto-connect on startup).</param>
    Task<bool> ConnectWithProfileAsync(
        ConnectionProfile profile,
        CancellationToken cancellationToken = default,
        bool showErrors = true);

    /// <summary>Displays a message box to the user.</summary>
    void ShowMessage(string message, string title, MessageType messageType = MessageType.Info);
}
