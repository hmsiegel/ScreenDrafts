namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

internal sealed class MovieAddedDomainEventHandler(ISender sender, IEventBus eventBus)
  : DomainEventHandler<MovieCreatedDomainEvent>
{
  private readonly ISender _sender = sender;
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(
    MovieCreatedDomainEvent domainEvent,
    CancellationToken cancellationToken = default)
  {
    var result = await _sender.Send(new GetMovieQuery(domainEvent.ImdbId), cancellationToken);

    if (result.IsFailure)
    {
      throw new ScreenDraftsException(nameof(GetMovieQuery), result.Error);
    }

    await _eventBus.PublishAsync(
      new MovieAddedIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        movieId: result.Value.Id,
        title: result.Value.Title,
        imdbId: result.Value.ImdbId,
        tmdbId: result.Value.TmdbId),
      cancellationToken);
  }
}
