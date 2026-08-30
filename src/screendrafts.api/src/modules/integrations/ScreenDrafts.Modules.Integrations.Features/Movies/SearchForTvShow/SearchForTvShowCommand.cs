namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchForTvShow;

internal sealed record SearchForTvShowCommand : ICommand<SearchForTvShowResponse>
{
  public int Page { get; init; } = 1;
  public string Query { get; init; } = string.Empty;
}
