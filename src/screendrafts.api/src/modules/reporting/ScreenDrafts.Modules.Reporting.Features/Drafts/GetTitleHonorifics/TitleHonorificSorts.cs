namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

internal static class TitleHonorificSorts
{
  public const string Newest = "newest";
  public const string Oldest = "oldest";
  public const string Alphabetical = "alphabetical";
  public const string Appearances = "appearances";

  public static bool IsKnown(string sort) =>
    sort is Newest or Oldest or Alphabetical or Appearances;
}
