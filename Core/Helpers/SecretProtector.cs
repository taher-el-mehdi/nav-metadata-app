using System.Security.Cryptography;
using System.Text;

namespace NAVMetadata.Helpers;

/// <summary>Encrypts secrets for local storage using Windows DPAPI (current user).</summary>
internal static class SecretProtector
{
    public static string? Protect(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return null;

        var bytes = Encoding.UTF8.GetBytes(plainText);
        var protectedBytes = ProtectedData.Protect(bytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    public static string? Unprotect(string? protectedText)
    {
        if (string.IsNullOrEmpty(protectedText))
            return null;

        try
        {
            var bytes = Convert.FromBase64String(protectedText);
            var plainBytes = ProtectedData.Unprotect(bytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            return null;
        }
    }
}
