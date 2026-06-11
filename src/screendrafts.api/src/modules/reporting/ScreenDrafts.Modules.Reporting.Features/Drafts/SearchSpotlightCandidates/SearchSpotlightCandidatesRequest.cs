namespace ScreenDrafts.Modules.Reporting.Features.Drafts.SearchSpotlightCandidates;

internal sealed record SearchSpotlightCandidatesRequest
{
  [FromQuery(Name = "query")]
  public string? Query { get; init; }

  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; } = 20;
}
