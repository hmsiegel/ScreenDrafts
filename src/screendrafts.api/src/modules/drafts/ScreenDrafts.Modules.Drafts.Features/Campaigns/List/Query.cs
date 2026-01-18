using ScreenDrafts.Modules.Drafts.Features.Series.List;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.List;

internal sealed record Query(bool IncludeDeleted) : IQuery<CampaignCollectionResponse>;

