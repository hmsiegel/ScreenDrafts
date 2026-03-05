namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class PickCreatedDomainEventHandler(IEventBus eventBus) : DomainEventHandler<PickAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(PickAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _eventBus.PublishAsync(new PickAddedIntegrationEvent(
      domainEvent.Id,
      domainEvent.OccurredOnUtc,
      domainEvent.DraftPartId), 
      cancellationToken);
  }
}
