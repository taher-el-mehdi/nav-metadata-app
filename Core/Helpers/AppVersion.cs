using System.Reflection;

namespace NAVMetadata.Helpers;

/// <summary>Reads the application version from the assembly metadata.</summary>
public static class AppVersion
{
    private static string? _cached;

    public static string Current => _cached ??= Resolve();

    private static string Resolve()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version is null)
            return "0.0.0";

        return version.Revision >= 0
            ? $"{version.Major}.{version.Minor}.{version.Build}"
            : $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
