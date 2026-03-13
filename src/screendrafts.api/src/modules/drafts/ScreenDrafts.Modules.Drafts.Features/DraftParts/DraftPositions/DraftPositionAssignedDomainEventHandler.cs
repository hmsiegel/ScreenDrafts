namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions;

internal sealed class DraftPositionAssignedDomainEventHandler(IEventBus eventBus)
  : DomainEventHandler<DraftPositionAssignedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(DraftPositionAssignedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _eventBus.PublishAsync(
      new DraftPositionAssignedIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        draftPartId: domainEvent.DraftPartId,
        draftPositionId: domainEvent.DraftPositionId,
        participantId: domainEvent.ParticipantId,
        participantKind: domainEvent.ParticipantKind),
      cancellationToken);
  }
}
