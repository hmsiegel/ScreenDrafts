namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ClearCampaign;

internal sealed record Request
{
  public required string DraftId { get; init; }
}
