namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchFoMovie;

internal sealed record SearchFoMovieCommand : ICommand<SearchForMovieResponse>
{
  public int Page { get; init; } = 1;
  public string Query { get; init; } = string.Empty;
}
