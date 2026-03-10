namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchFoMovie;

internal sealed record SearchFoMovieCommand : ICommand<SearchForMovieResponse>
{
  public string Query { get; init; } = string.Empty;
}
