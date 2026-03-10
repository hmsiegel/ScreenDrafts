namespace ScreenDrafts.Modules.Integrations.Domain.Imdb;

public sealed record TmdbFindResult
{
  public IReadOnlyList<TmdbMovieSearchResult> MovieResults { get; init; } = default!;
}
