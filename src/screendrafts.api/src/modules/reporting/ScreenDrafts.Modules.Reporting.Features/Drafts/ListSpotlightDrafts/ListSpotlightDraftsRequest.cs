namespace ScreenDrafts.Modules.Reporting.Features.Drafts.ListSpotlightDrafts;

internal sealed record ListSpotlightDraftsRequest
{
  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; } = 5;

  [FromQuery(Name = "excludeActive")]
  public bool ExcludeActive { get; init; } = false;

  [FromQuery(Name = "query")]
  public string? Query { get; init; }

  [FromQuery(Name = "draftType")]
  public string? DraftType { get; init; }
}
