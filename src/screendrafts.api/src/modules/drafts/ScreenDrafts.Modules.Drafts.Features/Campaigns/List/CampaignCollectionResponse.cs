using ScreenDrafts.Modules.Drafts.Features.Series;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.List;

internal sealed record CampaignCollectionResponse(IReadOnlyList<CampaignResponse> Items);

