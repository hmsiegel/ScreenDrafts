namespace ScreenDrafts.Modules.Movies.Domain.Medias.DomainEvents;

public sealed class MediaCreatedDomainEvent(
  Guid mediaId,
  string? imdbId,
  int? tmdbId,
  int? igdbId,
  string publicId,
  MediaType mediaType) : DomainEvent
{
  public Guid MediaId { get; init; } = mediaId;
  public string PublicId { get; init; } = publicId;
  public string? ImdbId { get; init; } = imdbId;
  public int? TmdbId { get; init; } = tmdbId;
  public int? IgdbId { get; init; } = igdbId;
  public MediaType MediaType { get; init; } = mediaType;
}
