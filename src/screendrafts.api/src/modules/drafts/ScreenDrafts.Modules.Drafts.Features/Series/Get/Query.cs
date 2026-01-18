namespace ScreenDrafts.Modules.Drafts.Features.Series.Get;

internal sealed record Query(string PublicId) : IQuery<SeriesResponse>;
