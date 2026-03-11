namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class VetoAppliedDomainEventHandler(
  IEventBus eventBus,
  ICacheService cacheService)
  : DomainEventHandler<VetoAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly ICacheService _cacheService = cacheService;

  public override async Task Handle(VetoAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _cacheService.RemoveAsync(
      key: DraftsCacheKeys.PickList(draftPartPublicId: domainEvent.DraftPartPublicId),
      cancellationToken: cancellationToken);

    await _eventBus.PublishAsync(new VetoAppliedIntegrationEvent(
      domainEvent.Id,
      domainEvent.OccurredOnUtc,
      domainEvent.DraftPartId),
      cancellationToken);
  }
}
