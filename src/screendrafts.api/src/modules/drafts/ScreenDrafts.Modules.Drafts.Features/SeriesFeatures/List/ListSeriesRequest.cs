namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.List;

internal sealed record ListSeriesRequest
{
  [FromQuery(Name = "includeDeleted")]
  public bool IncludeDeleted { get; init; }
}
