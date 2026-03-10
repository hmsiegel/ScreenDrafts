namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record SearchMoviesApiResponse
{
  public IReadOnlyList<MovieSearchApiResult> Results { get; init; } = [];
}
