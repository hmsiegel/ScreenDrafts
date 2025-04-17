namespace ScreenDrafts.Modules.Movies.Domain.Movies.DomainEvents;

public sealed class MovieCreatedDomainEvent(Guid movieId, string imdbId) : DomainEvent
{
  public Guid MovieId { get; init; } = movieId;

  public string ImdbId { get; init; } = imdbId;
}
