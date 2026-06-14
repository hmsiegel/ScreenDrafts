namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class VetoUndoDomainEventHandler(IEventBus eventBus)
  : DomainEventHandler<VetoUndoDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(
    VetoUndoDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      integrationEvent: new VetoUndoneIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: domainEvent.DraftPartPublicId,
        playOrder: domainEvent.PlayOrder,
        tmdbId: domainEvent.TmdbId,
        movieTitle: domainEvent.MovieTitle ?? string.Empty
      ),
      cancellationToken: cancellationToken
    );
  }
}
