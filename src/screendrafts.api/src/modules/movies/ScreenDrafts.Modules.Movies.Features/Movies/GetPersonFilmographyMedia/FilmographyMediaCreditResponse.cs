namespace ScreenDrafts.Modules.Movies.Features.Movies.GetPersonFilmographyMedia;

internal sealed record FilmographyMediaCreditResponse
{
  public int TmdbId { get; init; }
  public string Title { get; init; } = default!;
  public string? Year { get; init; }
  public string? PosterUrl { get; init; }
  public int MediaType { get; init; }
  public string? CreditRole { get; init; }
  public bool IsInMediaDatabase { get; init; }
  public string? MediaPublicId { get; init; }
}
