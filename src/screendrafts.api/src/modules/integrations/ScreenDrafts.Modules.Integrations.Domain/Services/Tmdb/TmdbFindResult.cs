namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

public sealed record TmdbFindResult
{
  public IReadOnlyList<TmdbSearchResult> MovieResults { get; init; } = [];
  public IReadOnlyList<TmdbTvResult> TvResults { get; init; } = [];
  public IReadOnlyList<TmdbTvEpisodeResult> TvEpisodeResults { get; init; } = [];

  public bool HasMovie => MovieResults.Count > 0;
  public bool HasTvShow => TvResults.Count > 0;
  public bool HasTvEpisode => TvEpisodeResults.Count > 0;
  public bool IsUnknown => !HasMovie && !HasTvShow && !HasTvEpisode;

  public TmdbSearchResult? MovieResult => MovieResults[0];
  public TmdbTvResult? TvResult => TvResults[0];
  public TmdbTvEpisodeResult? TvEpisodeResult => TvEpisodeResults[0];
}
