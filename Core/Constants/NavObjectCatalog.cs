using NAVMetadata.Enums;

namespace NAVMetadata.Constants;

/// <summary>
/// NAV object types exposed in the UI and loaded into the workspace.
/// Add new types here and in the sidebar — both use this list.
/// </summary>
public static class NavObjectCatalog
{
    public static readonly IReadOnlyList<ObjectType> BrowsableTypes =
    [
        ObjectType.Table,
        ObjectType.Page,
        ObjectType.Report,
        ObjectType.Query
    ];
}
