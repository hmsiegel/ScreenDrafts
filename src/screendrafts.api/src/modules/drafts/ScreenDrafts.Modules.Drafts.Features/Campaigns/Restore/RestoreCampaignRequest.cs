namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

internal sealed record RestoreCampaignRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}

