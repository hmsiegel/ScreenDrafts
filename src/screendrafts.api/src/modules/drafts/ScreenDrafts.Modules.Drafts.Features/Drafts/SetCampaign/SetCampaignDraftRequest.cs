namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;

internal sealed record SetCampaignDraftRequest
{
  [FromRoute(Name = "publicId")]
  public string DraftId { get; init; } = default!;

  [FromBody]
  public string CampaignId { get; init; } = default!;
}

