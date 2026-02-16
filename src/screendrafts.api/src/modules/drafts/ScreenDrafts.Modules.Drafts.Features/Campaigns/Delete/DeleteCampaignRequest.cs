namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

internal sealed record DeleteCampaignRequest
{
  public required string PublicId { get; init; }
}

