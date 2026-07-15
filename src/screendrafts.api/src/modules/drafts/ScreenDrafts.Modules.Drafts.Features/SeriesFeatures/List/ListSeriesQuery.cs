namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.List;

internal sealed record ListSeriesQuery : IQuery<SeriesCollectionResponse>
{
  public bool IncludeDeleted { get; init; }
}
