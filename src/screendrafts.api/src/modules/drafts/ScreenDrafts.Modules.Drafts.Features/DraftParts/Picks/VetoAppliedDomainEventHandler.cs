namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class VetoAppliedDomainEventHandler(IEventBus eventBus) : DomainEventHandler<VetoAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(VetoAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _eventBus.PublishAsync(new VetoAppliedIntegrationEvent(
      domainEvent.Id,
      domainEvent.OccurredOnUtc,
      domainEvent.DraftPartId), 
      cancellationToken);
  }
}
