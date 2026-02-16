namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Edit;

internal sealed record EditCampaignRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = string.Empty;
  public string? Name { get; init; }
  public string? Slug { get; init; }
}

