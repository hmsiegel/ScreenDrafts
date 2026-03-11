namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class CommissionerOverrideAppliedDomainEventHandler(
  IEventBus eventBus,
  ICacheService cacheService)
  : DomainEventHandler<CommissionerOverrideAppliedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly ICacheService _cacheService = cacheService;

  public override async Task Handle(CommissionerOverrideAppliedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _cacheService.RemoveAsync(
      key: DraftsCacheKeys.PickList(domainEvent.DraftPartPublicId),
      cancellationToken: cancellationToken);

    await _eventBus.PublishAsync(new CommissionerOverrideAppliedIntegrationEvent(
      domainEvent.Id,
      domainEvent.OccurredOnUtc,
      domainEvent.DraftPartId),
      cancellationToken);
  }
}
