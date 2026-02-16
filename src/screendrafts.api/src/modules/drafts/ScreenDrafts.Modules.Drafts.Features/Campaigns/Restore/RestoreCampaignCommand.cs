namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

internal sealed record RestoreCampaignCommand(string PublicId) : ICommand;

