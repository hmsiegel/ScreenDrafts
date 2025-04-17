namespace ScreenDrafts.Modules.Movies.Application.Movies.Commands.AddMovie;

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
        domainEvent.Id,
        domainEvent.OccurredOnUtc,
        result.Value.Id,
        result.Value.Title,
        result.Value.ImdbId),
      cancellationToken);
  }
}
