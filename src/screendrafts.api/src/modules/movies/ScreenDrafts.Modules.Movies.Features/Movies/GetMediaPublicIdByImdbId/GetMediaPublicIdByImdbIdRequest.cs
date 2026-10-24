namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPublicIdByImdbId;

// Resolves one IMDb ID to a media public ID. Anonymous: the website uses it to redirect
// legacy /media/tt... URLs to the current /media/{publicId} page.

internal sealed record GetMediaPublicIdByImdbIdRequest
{
  [FromRoute(Name = "imdbId")]
  public string ImdbId { get; init; } = default!;
}
