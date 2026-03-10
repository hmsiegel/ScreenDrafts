namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMovies;

internal sealed record SearchMoviesResponse
{
  public PagedResult<MovieSearchResultResponse> Results { get; init; } = default!;
}
