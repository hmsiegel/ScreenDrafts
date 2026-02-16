namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;

internal sealed record SetCampaignDraftRequest
{
  [FromRoute(Name = "draftId")]
  public string DraftId { get; init; } = default!;

  [Microsoft.AspNetCore.Mvc.FromBody]
  public string CampaignId { get; init; } = default!;
}

