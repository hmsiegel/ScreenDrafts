namespace ScreenDrafts.Modules.Movies.IntegrationEvents;

public sealed class MediaAddedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid mediaId,
  string title,
  string? imdbId,
  int? tmdbId,
  string publicId,
  MediaType mediaType,
  int? igdbId,
  string? year,
  int? tvSeriesTmdbId = null,
  int? seasonNumber = null,
  int? episodeNumber = null,
  string? tvSeriesTitle = null
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid MediaId { get; init; } = mediaId;
  public string PublicId { get; init; } = publicId;
  public string Title { get; init; } = title;
  public string? ImdbId { get; init; } = imdbId;
  public int? TmdbId { get; init; } = tmdbId;
  public int? IgdbId { get; init; } = igdbId;
  public MediaType MediaType { get; init; } = mediaType;
  public string? Year { get; init; } = year;
  public int? TvSeriesTmdbId { get; init; } = tvSeriesTmdbId;
  public int? SeasonNumber { get; init; } = seasonNumber;
  public int? EpisodeNumber { get; init; } = episodeNumber;
  public string? TvSeriesTitle { get; init; } = tvSeriesTitle;
}
