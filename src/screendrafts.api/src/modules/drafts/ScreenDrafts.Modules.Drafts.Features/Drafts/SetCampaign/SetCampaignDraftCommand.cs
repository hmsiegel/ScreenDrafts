namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;

internal sealed record SetCampaignDraftCommand : ICommand
{
  public string DraftId { get; init; } = default!;
  public string CampaignId { get; init; } = default!;
}

