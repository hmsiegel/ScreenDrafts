namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchFoMovie;

internal sealed record SearchForMovieResponse
{
  public IReadOnlyList<MovieSearchResult> Results { get; set; } = [];
}
