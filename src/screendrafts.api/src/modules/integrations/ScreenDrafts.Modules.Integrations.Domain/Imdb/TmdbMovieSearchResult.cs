namespace ScreenDrafts.Modules.Integrations.Domain.Imdb;

public sealed record TmdbMovieSearchResult
{
  public int Id { get; init; }
  public string Title { get; init; } = string.Empty;
  public string Overview { get; init; } = string.Empty;
  public string? PosterPath { get; init;  } = string.Empty;
  public string? ReleaseDate { get; init; }
}
