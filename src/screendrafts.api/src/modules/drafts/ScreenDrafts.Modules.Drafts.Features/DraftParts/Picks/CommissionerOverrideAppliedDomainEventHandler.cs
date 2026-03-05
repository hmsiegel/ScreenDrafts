namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class CommissionerOverrideAppliedDomainEventHandler(IEventBus eventBus) : DomainEventHandler<CommissionerOverrideAppliedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(CommissionerOverrideAppliedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _eventBus.PublishAsync(new CommissionerOverrideAppliedIntegrationEvent(
      domainEvent.Id,
      domainEvent.OccurredOnUtc,
      domainEvent.DraftPartId), 
      cancellationToken);
  }
}
