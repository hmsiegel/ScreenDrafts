namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchForMovie;

internal sealed record SearchForMovieCommand : ICommand<SearchForMovieResponse>
{
  public int Page { get; init; } = 1;
  public string Query { get; init; } = string.Empty;
}
