namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

internal sealed record GetCampaignQuery(string PublicId, bool IncludeDeleted = false) : IQuery<CampaignResponse>;

