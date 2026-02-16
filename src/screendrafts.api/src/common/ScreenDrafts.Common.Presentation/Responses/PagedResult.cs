namespace ScreenDrafts.Common.Presentation.Responses;

public sealed record PagedResult<T> : ICollectionResponse<T>
{
  public required Collection<T> Items { get; init; } = [];
  public required int TotalCount { get; init; }
  public required int Page { get; init; }
  public required int PageSize { get; init; }

  public int TotalPages =>
    (int)Math.Ceiling((double)TotalCount / PageSize);
  public bool HasPreviousPage => Page > 1;
  public bool HasNextPage => Page < TotalPages;

  public static PagedResult<T> Create(
    IQueryable<T> query,
    int page,
    int pageSize)
  {
    var totalCount = query.Count();
    var items = new Collection<T>(
      [.. query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)]
    );

    return new()
    {
      Items = items,
      TotalCount = totalCount,
      Page = page,
      PageSize = pageSize
    };
  }
}
