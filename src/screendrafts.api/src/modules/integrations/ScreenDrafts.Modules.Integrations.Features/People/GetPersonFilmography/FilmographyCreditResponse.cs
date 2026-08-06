namespace ScreenDrafts.Modules.Integrations.Features.People.GetPersonFilmography;

internal sealed record FilmographyCreditResponse
{
  public int TmdbId { get; init; }
  public string Title { get; init; } = string.Empty;
  public string? Year { get; init; }
  public string? PosterUrl { get; init; }

  /// <summary>
  /// 0 = Movie, 1 = TV
  /// </summary>
  public int MediaType { get; init; }

  /// <summary>e.g. "Actor", "Director", "Writer" (IMDbApiLib's CastMovie.Role) — display only, not filtered on.</summary>
  public string? CreditRole { get; init; }
}
