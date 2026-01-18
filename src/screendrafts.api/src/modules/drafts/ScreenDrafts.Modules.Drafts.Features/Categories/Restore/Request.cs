namespace ScreenDrafts.Modules.Drafts.Features.Categories.Restore;

internal sealed record Request
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}
