namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Search;

internal sealed record SearchDrafterTeamsRequest
{
  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; } = 10;

  [FromQuery(Name = "name")]
  public string? Name { get; init; }
}
