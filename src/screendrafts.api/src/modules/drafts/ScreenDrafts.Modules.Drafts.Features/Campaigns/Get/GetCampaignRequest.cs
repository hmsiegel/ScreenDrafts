namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

internal sealed record GetCampaignRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}

