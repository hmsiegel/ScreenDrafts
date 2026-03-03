namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Search;

internal sealed record SearchDrafterTeamsQuery : IQuery<PagedResult<SearchDrafterTeamsResponse>>
{
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 10;
  public string? Name { get; init; }
}
