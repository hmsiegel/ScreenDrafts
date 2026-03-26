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
  int? igdbId)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid MediaId { get; init; } = mediaId;
  public string PublicId { get; init; } = publicId;
  public string Title { get; init; } = title;
  public string? ImdbId { get; init; } = imdbId;
  public int? TmdbId { get; init; } = tmdbId;
  public int? IgdbId { get; init; } = igdbId;
  public MediaType MediaType { get; init; } = mediaType;
}
