using NuGet.Versioning;

namespace NAVMetadata.Helpers;

/// <summary>Compares version strings using Semantic Versioning rules.</summary>
public static class SemanticVersionComparer
{
    public static bool TryParse(string? version, out NuGetVersion parsed) =>
        NuGetVersion.TryParse(Normalize(version), out parsed!);

    public static bool IsNewer(string? latest, string? current)
    {
        if (!TryParse(latest, out var latestVersion))
            return false;

        if (!TryParse(current, out var currentVersion))
            return true;

        return latestVersion > currentVersion;
    }

    public static string Normalize(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return "0.0.0";

        var trimmed = version.Trim();
        return trimmed.StartsWith("v", StringComparison.OrdinalIgnoreCase)
            ? trimmed[1..]
            : trimmed;
    }
}
