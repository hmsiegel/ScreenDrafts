namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Search;

internal sealed record SearchHostRequest
{
  [FromQuery(Name = "name")]
  public string? Name { get; init; }

  [FromQuery(Name = "page")]
  public int? Page { get; init; }

  [FromQuery(Name = "pageSize")]
  public int? PageSize { get; init; }

  [FromQuery(Name = "sortBy")]
  public string? SortBy { get; init; }

  [FromQuery(Name = "hasBeenPrimary")]
  public bool? HasBeenPrimary { get; init; }
}
