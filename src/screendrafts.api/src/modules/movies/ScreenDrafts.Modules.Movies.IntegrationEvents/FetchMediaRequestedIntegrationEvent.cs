namespace ScreenDrafts.Modules.Movies.IntegrationEvents;

public sealed class FetchMediaRequestedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  int tmdbId,
  int? igdbId,
  int? tvSeriesTmdbId,
  int? seasonNumber,
  int? episodeNumber,
  MediaType mediaType,
  string? imdbId)
  : IntegrationEvent(id, occurredOnUtc)
{
  public int? TmdbId { get; init; } = tmdbId;
  public int? IgdbId { get; init; } = igdbId;
  public string? ImdbId { get; init; } = imdbId;
  public int? TvSeriesTmdbId { get; init; } = tvSeriesTmdbId;
  public int? SeasonNumber { get; init; } = seasonNumber;
  public int? EpisodeNumber { get; init; } = episodeNumber;
  public MediaType MediaType { get; init; } = mediaType;
}
