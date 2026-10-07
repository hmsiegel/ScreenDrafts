namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

public static class TitleHonorificErrors
{
  public static SDError UnknownLevel(string level) =>
    SDError.NotFound("TitleHonorifics.UnknownLevel", $"There is no honorific level '{level}'.");

  public static SDError UnknownSort(string sort) =>
    SDError.Problem("TitleHonorifics.UnknownSort", $"Unknown sort '{sort}'.");

  public static readonly SDError InvalidPage = SDError.Problem(
    "TitleHonorifics.InvalidPage",
    "Page must be 1 or higher."
  );

  public static readonly SDError InvalidPageSize = SDError.Problem(
    "TitleHonorifics.InvalidPageSize",
    "Page size must be between 1 and 100."
  );

  public static readonly SDError SearchTooLong = SDError.Problem(
    "TitleHonorifics.SearchTooLong",
    "Search text can be at most 100 characters."
  );
}
