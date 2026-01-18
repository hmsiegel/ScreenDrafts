namespace ScreenDrafts.Modules.Drafts.Features.Series.Get;

internal sealed record Request
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}
