namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.AssignParticipantToDraftPosition;

internal sealed class AllPositionsAssignedDomainEventHandler(IEventBus eventBus)
  : DomainEventHandler<AllPositionsAssignedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(
    AllPositionsAssignedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      new PositionsSetIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: domainEvent.DraftPartPublicId
      ),
      cancellationToken
    );
  }
}
