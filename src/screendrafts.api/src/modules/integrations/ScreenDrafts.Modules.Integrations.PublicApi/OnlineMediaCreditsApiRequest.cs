namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record OnlineMediaCreditsApiRequest
{
  /// <summary>MediaType value: 0 = Movie, 1 = TV show, 2 = TV episode.</summary>
  public int MediaType { get; init; }
  public int? TmdbId { get; init; }
  public string? ImdbId { get; init; }
  public int? TvSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
}
