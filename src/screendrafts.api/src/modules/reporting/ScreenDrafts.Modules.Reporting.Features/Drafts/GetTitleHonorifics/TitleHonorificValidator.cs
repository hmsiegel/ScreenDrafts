namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

internal static class TitleHonorificValidator
{
  public const int DefaultPageSize = 50;
  public const int MaxPageSize = 100;
  private const int MaxSearchLength = 100;

  public static TitleHonorificValidation Validate(GetTitleHonorificsQuery query)
  {
    ArgumentNullException.ThrowIfNull(query);

    var level = TitleHonorificLevels.Find(query.Level);

    if (level is null)
    {
      return TitleHonorificValidation.Fail(TitleHonorificErrors.UnknownLevel(query.Level));
    }

    var sort = string.IsNullOrWhiteSpace(query.Sort) ? TitleHonorificSorts.Newest : query.Sort;

    if (!TitleHonorificSorts.IsKnown(sort))
    {
      return TitleHonorificValidation.Fail(TitleHonorificErrors.UnknownSort(sort));
    }

    var page = query.Page ?? 1;

    if (page < 1)
    {
      return TitleHonorificValidation.Fail(TitleHonorificErrors.InvalidPage);
    }

    var pageSize = query.PageSize ?? DefaultPageSize;

    if (pageSize is < 1 or > MaxPageSize)
    {
      return TitleHonorificValidation.Fail(TitleHonorificErrors.InvalidPageSize);
    }

    if (query.Search is { Length: > MaxSearchLength })
    {
      return TitleHonorificValidation.Fail(TitleHonorificErrors.SearchTooLong);
    }

    return TitleHonorificValidation.Ok(
      new TitleHonorificSpec(level, query.Search, sort, page, pageSize)
    );
  }
}
