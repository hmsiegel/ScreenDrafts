namespace ScreenDrafts.Modules.Movies.Domain.Movies.DomainEvents;

public sealed class MovieCreatedDomainEvent(Guid movieId) : DomainEvent
{
  public Guid MovieId { get; init; } = movieId;
}
