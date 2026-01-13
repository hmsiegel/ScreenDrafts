namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

internal sealed record Request
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}
