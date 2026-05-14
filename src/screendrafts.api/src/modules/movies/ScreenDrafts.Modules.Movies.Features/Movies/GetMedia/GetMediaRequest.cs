namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

internal sealed record GetMediaRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
