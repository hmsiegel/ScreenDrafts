namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ClearCampaign;

internal sealed record ClearCampaignDraftCommand : ICommand
{
  public string DraftId { get; init; } = default!;
}

