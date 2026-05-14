namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

internal sealed record RestoreCampaignRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}

