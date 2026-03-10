namespace ScreenDrafts.Modules.Integrations.Domain.Imdb;

public sealed record TmdbMovieDetails
{
  public int Id { get; init; }
  public string Title { get; init; } = string.Empty;
  public string Overview { get; init; } = string.Empty;
  public string? PosterPath { get; init; }
  public string? ReleaseDate { get; init; }
  public TmdbCredits Credits { get; init; } = default!;
}
