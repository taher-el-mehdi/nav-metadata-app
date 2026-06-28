using NAVMetadata.Models;

namespace NAVMetadata.Helpers;

/// <summary>
/// Filters NAV objects by ID and name — same idea as NAV Object Designer column filters.
/// </summary>
public static class ObjectListFilter
{
    /// <summary>
    /// Returns objects matching the optional ID and name filters.
    /// ID: exact match when numeric, otherwise prefix match on the ID string.
    /// Name: case-insensitive substring match.
    /// </summary>
    public static IEnumerable<NavObject> Apply(
        IEnumerable<NavObject> source,
        string? idFilter,
        string? nameFilter)
    {
        var id = idFilter?.Trim();
        var name = nameFilter?.Trim();
        var hasId = !string.IsNullOrEmpty(id);
        var hasName = !string.IsNullOrEmpty(name);

        if (!hasId && !hasName)
            return source;

        return source.Where(obj =>
            (!hasId || MatchesId(obj, id!)) &&
            (!hasName || MatchesName(obj, name!)));
    }

    private static bool MatchesId(NavObject obj, string filter) =>
        int.TryParse(filter, out var exactId)
            ? obj.ObjectId == exactId
            : obj.ObjectId.ToString().StartsWith(filter, StringComparison.Ordinal);

    private static bool MatchesName(NavObject obj, string filter) =>
        obj.ObjectName.Contains(filter, StringComparison.OrdinalIgnoreCase);
}
