namespace ScreenDrafts.Modules.Movies.Domain.Movies.DomainEvents;

public sealed class MovieCreatedDomainEvent(
  Guid movieId,
  string imdbId,
  int tmdbId) : DomainEvent
{
  public Guid MovieId { get; init; } = movieId;
  public string ImdbId { get; init; } = imdbId;
  public int TmdbId { get; init; } = tmdbId;
}
