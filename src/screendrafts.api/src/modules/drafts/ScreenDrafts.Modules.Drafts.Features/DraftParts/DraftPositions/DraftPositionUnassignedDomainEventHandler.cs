namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions;

internal sealed class DraftPositionUnassignedDomainEventHandler(IEventBus eventBus)
  : DomainEventHandler<DraftPositionUnassignedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(DraftPositionUnassignedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _eventBus.PublishAsync(
      new DraftPositionUnassignedIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        draftPartId: domainEvent.DraftPartId,
        draftPositionId: domainEvent.DraftPositionId), 
      cancellationToken);
  }
}
