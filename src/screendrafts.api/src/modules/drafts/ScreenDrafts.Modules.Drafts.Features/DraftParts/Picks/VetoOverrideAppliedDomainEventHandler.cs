namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class VetoOverrideAppliedDomainEventHandler(IEventBus eventBus) : DomainEventHandler<VetoOverrideAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(VetoOverrideAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _eventBus.PublishAsync(new VetoOverrideAppliedIntegrationEvent(
      domainEvent.Id,
      domainEvent.OccurredOnUtc,
      domainEvent.DraftPartId), 
      cancellationToken);
  }
}
