namespace ScreenDrafts.Modules.Drafts.Domain.WikiExport;

public static class WikiExportErrors
{
  public static readonly SDError NoDraftsFound = SDError.NotFound(
    "WikiExport.NoDraftsFound",
    "None of the selected drafts were found."
  );

  public static readonly SDError NoDraftersFound = SDError.NotFound(
    "WikiExport.NoDraftersFound",
    "None of the selected drafters were found."
  );
}
