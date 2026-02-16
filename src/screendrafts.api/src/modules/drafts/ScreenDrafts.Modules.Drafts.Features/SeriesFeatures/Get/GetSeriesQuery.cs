namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Get;

internal sealed record GetSeriesQuery(string PublicId) : IQuery<SeriesResponse>;

