using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures;

namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.List;

internal sealed record SeriesCollectionResponse(IReadOnlyList<SeriesResponse> Items);

