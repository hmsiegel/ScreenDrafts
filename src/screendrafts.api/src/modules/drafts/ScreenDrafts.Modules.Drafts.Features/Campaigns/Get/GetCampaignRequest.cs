namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

internal sealed record GetCampaignRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  [FromQuery]
  public bool IncludeDeleted { get; init; }
}

