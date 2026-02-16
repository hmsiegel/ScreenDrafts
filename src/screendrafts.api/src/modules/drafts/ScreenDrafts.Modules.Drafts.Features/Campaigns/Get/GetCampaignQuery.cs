namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

internal sealed record GetCampaignQuery(string PublicId) : IQuery<CampaignResponse>;

