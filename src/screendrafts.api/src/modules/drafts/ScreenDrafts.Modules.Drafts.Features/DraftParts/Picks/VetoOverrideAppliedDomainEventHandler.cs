namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class VetoOverrideAppliedDomainEventHandler(
  IEventBus eventBus,
  ICacheService cacheService)
  : DomainEventHandler<VetoOverrideAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly ICacheService _cacheService = cacheService;

  public override async Task Handle(VetoOverrideAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _cacheService.RemoveAsync(
      key: DraftsCacheKeys.PickList(domainEvent.DraftPartPublicId),
      cancellationToken: cancellationToken);

    await _eventBus.PublishAsync(new VetoOverrideAppliedIntegrationEvent(
      domainEvent.Id,
      domainEvent.OccurredOnUtc,
      domainEvent.DraftPartId),
      cancellationToken);
  }
}
