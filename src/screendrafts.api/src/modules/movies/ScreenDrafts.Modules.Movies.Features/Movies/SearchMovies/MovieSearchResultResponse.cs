namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMovies;

internal sealed record MovieSearchResultResponse
{
  public int TmdbId { get; init; }
  public string Title { get; init; } = default!;
  public string? Year { get; init; }
  public string? PosterUrl { get; init; }
  public string? Overview { get; init; }
  public bool IsInMoviesDatabase { get; init; }
}
