namespace ScreenDrafts.Modules.Movies.IntegrationEvents;

public sealed class FetchMediaRequestedIntegrationEvent : IntegrationEvent
{
  public FetchMediaRequestedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    int? tmdbId,
    int? igdbId,
    int? tvSeriesTmdbId,
    int? seasonNumber,
    int? episodeNumber,
    MediaType mediaType,
    string? imdbId
  )
    : base(id, occurredOnUtc)
  {
    TmdbId = tmdbId;
    IgdbId = igdbId;
    ImdbId = imdbId;
    TvSeriesTmdbId = tvSeriesTmdbId;
    SeasonNumber = seasonNumber;
    EpisodeNumber = episodeNumber;
    MediaType = mediaType;
  }

  // Parameterless constructor required for MassTransit deserialization
  public FetchMediaRequestedIntegrationEvent()
    : base(Guid.Empty, DateTime.MinValue) { }

  public int? TmdbId { get; init; }
  public int? IgdbId { get; init; }
  public string? ImdbId { get; init; }
  public int? TvSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
  public MediaType MediaType { get; init; } = default!;
}
