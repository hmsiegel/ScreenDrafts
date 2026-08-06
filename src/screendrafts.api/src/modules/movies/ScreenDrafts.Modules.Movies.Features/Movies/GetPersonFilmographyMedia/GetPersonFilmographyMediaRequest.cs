namespace ScreenDrafts.Modules.Movies.Features.Movies.GetPersonFilmographyMedia;

internal sealed record GetPersonFilmographyMediaRequest
{
  [FromQuery(Name = "imdbId")]
  public required string ImdbId { get; init; }
}
