namespace NAVMetadata.Constants;

/// <summary>
/// SQL statements for NAV system tables. Table names come from <see cref="AppConstants"/>.
/// Values are always passed via parameters — never concatenate user input into SQL.
/// </summary>
public static class NavSqlQueries
{
    /// <summary>Lists all objects of one type from the <c>[Object]</c> table.</summary>
    public static string ListObjectsByType => $"""
        SELECT [ID], [Name], [Modified], [Date], [Version List]
        FROM [{AppConstants.ObjectTable}]
        WHERE [Type] = @Type
        ORDER BY [ID]
        """;

    /// <summary>
    /// Decompresses gzip metadata from <c>[Object Metadata]</c>.
    /// Based on <see href="https://stackoverflow.com/a/75883186"/>.
    /// </summary>
    public static string DecompressObjectMetadata => $"""
        SELECT CONVERT(
            varchar(max),
            DECOMPRESS(
                CONVERT(varbinary(max), 0x1F8B0800000000000400)
                + CONVERT(varbinary(max), SUBSTRING([Metadata], 5, DATALENGTH([Metadata]) - 4))
            )
        )
        FROM [{AppConstants.ObjectMetadataTable}]
        WHERE [Object Type] = @ObjectType AND [Object ID] = @ObjectId
        """;

    /// <summary>Checks whether the connected database has the NAV <c>[Object]</c> table.</summary>
    public static string HasObjectTable => $"""
        SELECT CASE WHEN EXISTS (
            SELECT 1
            FROM sys.tables
            WHERE name = '{AppConstants.ObjectTable}'
        ) THEN 1 ELSE 0 END
        """;

    /// <summary>Lists online user databases on the server (run against <c>master</c>).</summary>
    public const string ListUserDatabases = """
        SELECT name
        FROM sys.databases
        WHERE state = 0
          AND database_id > 4
          AND HAS_DBACCESS(name) = 1
        ORDER BY name
        """;
}
