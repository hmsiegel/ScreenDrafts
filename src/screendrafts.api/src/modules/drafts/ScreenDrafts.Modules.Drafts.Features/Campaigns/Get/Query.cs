namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

internal sealed record Query(string PublicId) : IQuery<CampaignResponse>;
