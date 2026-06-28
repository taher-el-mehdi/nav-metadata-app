using NAVMetadata.Enums;
using NAVMetadata.Helpers;

namespace NAVMetadata.Models;

/// <summary>JSON-serializable connection settings stored on disk.</summary>
internal sealed class ConnectionSettingsDto
{
    public string ServerName { get; set; } = "";
    public string DatabaseName { get; set; } = "";
    public AuthenticationType AuthenticationType { get; set; }
    public string? Username { get; set; }
    public string? ProtectedPassword { get; set; }

    public static ConnectionSettingsDto From(ConnectionProfile profile) => new()
    {
        ServerName = profile.ServerName,
        DatabaseName = profile.DatabaseName,
        AuthenticationType = profile.AuthenticationType,
        Username = profile.Username,
        ProtectedPassword = SecretProtector.Protect(profile.Password)
    };

    public ConnectionProfile ToProfile() => new()
    {
        ServerName = ServerName,
        DatabaseName = DatabaseName,
        AuthenticationType = AuthenticationType,
        Username = Username,
        Password = SecretProtector.Unprotect(ProtectedPassword)
    };
}
