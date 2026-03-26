namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

internal sealed record GetMediaRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}
