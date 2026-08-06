namespace ScreenDrafts.Modules.Movies.Features.Movies.GetPersonFilmographyMedia;

internal sealed record GetPersonFilmographyMediaResponse
{
  public string PersonName { get; init; } = default!;
  public string? PersonPhotoUrl { get; init; }
  public IReadOnlyList<FilmographyMediaCreditResponse> Credits { get; init; } = [];
}
