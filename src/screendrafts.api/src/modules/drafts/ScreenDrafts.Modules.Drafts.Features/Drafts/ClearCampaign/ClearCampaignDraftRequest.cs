namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ClearCampaign;

internal sealed record ClearCampaignDraftRequest
{
  public required string DraftId { get; init; }
}

