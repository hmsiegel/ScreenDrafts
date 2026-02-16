namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.List;

internal sealed record ListCampaignsQuery(bool IncludeDeleted) : IQuery<CampaignCollectionResponse>;


