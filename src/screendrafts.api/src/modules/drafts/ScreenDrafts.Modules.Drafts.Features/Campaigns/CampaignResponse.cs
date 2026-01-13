namespace ScreenDrafts.Modules.Drafts.Features.Campaigns;

internal sealed record CampaignResponse
{
  public required string PublicId { get; init; }
  public required string Name { get; init; }
  public required string Slug { get; init; }
  public bool IsDeleted { get; init; }
}
