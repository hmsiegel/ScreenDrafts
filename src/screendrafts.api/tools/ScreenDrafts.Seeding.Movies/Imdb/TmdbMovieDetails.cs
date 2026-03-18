namespace ScreenDrafts.Seeding.Movies.Imdb;

public sealed record TmdbMovieDetails
{
  public int Id { get; init; }
  public string Title { get; init; } = string.Empty;
  public string Overview { get; init; } = string.Empty;
  public string? PosterPath { get; init; }
  public Uri? TrailerUrl { get; init; }
  public string? ReleaseDate { get; init; }
  public IReadOnlyList<TmdbGenre> Genres { get; init; } = [];
  public TmdbCredits Credits { get; init; } = default!;
}
