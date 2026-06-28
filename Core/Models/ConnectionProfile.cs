using Microsoft.Data.SqlClient;
using NAVMetadata.Constants;
using NAVMetadata.Enums;

namespace NAVMetadata.Models;

/// <summary>
/// SQL Server connection settings for a NAV database.
/// </summary>
public class ConnectionProfile
{
    public required string ServerName { get; set; }
    public required string DatabaseName { get; set; }
    public AuthenticationType AuthenticationType { get; set; } = AuthenticationType.Windows;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int ConnectionTimeoutSeconds { get; set; } = AppConstants.DefaultConnectionTimeoutSeconds;
    public int CommandTimeoutSeconds { get; set; } = AppConstants.DefaultCommandTimeoutSeconds;

    public string DisplayLabel => $"{ServerName} / {DatabaseName}";

    public string BuildConnectionString(string? databaseOverride = null)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = ServerName,
            InitialCatalog = databaseOverride ?? DatabaseName,
            ConnectTimeout = ConnectionTimeoutSeconds,
            Encrypt = true,
            TrustServerCertificate = true
        };

        if (AuthenticationType == AuthenticationType.SqlServer)
        {
            builder.UserID = Username;
            builder.Password = Password;
        }
        else
        {
            builder.IntegratedSecurity = true;
        }

        return builder.ConnectionString;
    }
}
