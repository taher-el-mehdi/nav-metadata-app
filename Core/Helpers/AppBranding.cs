using System.Reflection;

namespace NAVMetadata.Helpers;

/// <summary>Application icon and logo assets used across forms.</summary>
public static class AppBranding
{
    private const string LogoResourceName = "NAVMetadata.Logo.png";

    private static Icon? _applicationIcon;
    private static Image? _logoImage;

    public static Icon ApplicationIcon =>
        _applicationIcon ??= Icon.ExtractAssociatedIcon(Application.ExecutablePath)
            ?? SystemIcons.Application;

    public static Image LogoImage =>
        _logoImage ??= LoadLogoImage();

    public static void ApplyTo(Form form)
    {
        form.Icon = ApplicationIcon;
        form.ShowIcon = true;
    }

    private static Image LoadLogoImage()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(LogoResourceName)
            ?? throw new InvalidOperationException("Embedded logo resource was not found.");

        return Image.FromStream(stream);
    }
}
