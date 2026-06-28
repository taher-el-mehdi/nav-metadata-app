namespace NAVMetadata.Constants;

/// <summary>User-facing messages shown in dialogs and the welcome screen.</summary>
public static class AppMessages
{
    public const string NotNavDatabase =
        """
        The selected database does not appear to be a Microsoft Dynamics NAV database.

        Please choose a database that contains the [Object] system table used by NAV application objects.

        Typical database names look like: CRONUS or your company NAV database.
        """;

    public const string ReportIssuePrompt =
        "Would you like to report this issue on GitHub?";
}
