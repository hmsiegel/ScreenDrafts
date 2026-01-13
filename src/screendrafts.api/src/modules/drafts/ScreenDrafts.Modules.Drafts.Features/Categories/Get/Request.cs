namespace ScreenDrafts.Modules.Drafts.Features.Categories.Get;

internal sealed record Request
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}
