namespace NAVMetadata.Constants;

/// <summary>Consistent typography across the application.</summary>
public static class AppFonts
{
    public const string UiFamily = "Segoe UI";

    public static Font UiTitle { get; } = new(UiFamily, 22f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font UiSubtitle { get; } = new(UiFamily, 11f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font UiNormal { get; } = new(UiFamily, 10.5f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font UiSmall { get; } = new(UiFamily, 9.75f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font UiButton { get; } = new(UiFamily, 10.5f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font UiList { get; } = new(UiFamily, 10f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font UiBold { get; } = new(UiFamily, 10f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font Mono { get; } = CreateMono(10.5f);

    private static Font CreateMono(float size)
    {
        foreach (var family in new[] { "Cascadia Mono", "Consolas", "Courier New" })
        {
            try { return new Font(family, size, FontStyle.Regular, GraphicsUnit.Point); }
            catch (ArgumentException) { }
        }

        return new Font(FontFamily.GenericMonospace, size, FontStyle.Regular, GraphicsUnit.Point);
    }
}
