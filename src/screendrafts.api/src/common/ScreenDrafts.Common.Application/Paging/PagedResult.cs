namespace ScreenDrafts.Common.Application.Paging;

public sealed record PagedResult<T>(
  IReadOnlyCollection<T> Items,
  int Total,
  int Page,
  int PageSize);
