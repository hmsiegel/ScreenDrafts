namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

internal sealed record DeleteCampaignCommand(string PublicId) : ICommand;

