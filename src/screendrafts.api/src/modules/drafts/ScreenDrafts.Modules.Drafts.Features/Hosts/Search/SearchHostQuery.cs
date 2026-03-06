namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Search;

internal sealed record SearchHostQuery : IQuery<PagedResult<SearchHostResponse>>
{
  public string? Name { get; init; }
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 10;
  public string? SortBy { get; init; }
  public string? Dir { get; init; }
  public bool? HasBeenPrimary { get; init; }
}
