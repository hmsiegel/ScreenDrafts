namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchForMovie;

internal sealed record SearchForMovieResponse
{
  public IReadOnlyList<MovieSearchResult> Results { get; init; } = [];
  public int TotalResults { get; init; }
  public int TotalPages { get; init; }
  public int Page { get; init; }
}
