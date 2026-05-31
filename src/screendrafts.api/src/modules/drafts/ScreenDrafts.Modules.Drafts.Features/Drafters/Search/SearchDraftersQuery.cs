namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Search;

internal sealed record SearchDraftersQuery : IQuery<PagedResult<SearchDraftersResponse>>
{
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 10;
  public string? Name { get; init; }
  public bool? IsRetired { get; init; }
}
