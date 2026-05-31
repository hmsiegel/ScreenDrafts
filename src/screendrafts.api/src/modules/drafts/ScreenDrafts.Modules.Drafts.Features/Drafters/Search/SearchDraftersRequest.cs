namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Search;

internal sealed record SearchDraftersRequest
{
  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; } = 10;

  [FromQuery(Name = "name")]
  public string? Name { get; init; }

  [FromQuery(Name = "retired")]
  public bool? IsRetired { get; init; }
}
