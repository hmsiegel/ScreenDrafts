namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class PickUndoDomainEventHandler(IEventBus eventBus)
  : DomainEventHandler<PickUndoDomainEvent>
{
  public override async Task Handle(
    PickUndoDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await eventBus.PublishAsync(
      new PickUndoneIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: domainEvent.DraftPartPublicId,
        playOrder: domainEvent.PlayOrder,
        boardPosition: domainEvent.BoardPosition,
        tmdbId: domainEvent.TmdbId,
        movieTitle: domainEvent.MovieTitle
      ),
      cancellationToken
    );
  }
}
