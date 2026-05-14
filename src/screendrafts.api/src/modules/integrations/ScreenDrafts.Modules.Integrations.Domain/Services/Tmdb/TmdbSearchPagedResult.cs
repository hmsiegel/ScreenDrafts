namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

public sealed record TmdbSearchPagedResult
{
  public IReadOnlyList<TmdbSearchResult> Results { get; init; } = [];
  public int TotalResults { get; init; }
  public int TotalPages { get; init; }
  public int Page { get; init; }
}
