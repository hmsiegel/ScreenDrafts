namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddCampaignToDraft;

public sealed record AddCampaignToDraftCommand(Guid DraftId, Guid CampaignId) : ICommand;

