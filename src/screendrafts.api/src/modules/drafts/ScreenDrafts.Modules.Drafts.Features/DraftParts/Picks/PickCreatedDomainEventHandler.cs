namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class PickCreatedDomainEventHandler(
  IEventBus eventBus,
  ICacheService cacheService)
  : DomainEventHandler<PickAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly ICacheService _cacheService = cacheService;

  public override async Task Handle(PickAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await _cacheService.RemoveAsync(DraftsCacheKeys.PickList(domainEvent.DraftPartPublicId), cancellationToken);

    await _eventBus.PublishAsync(new PickAddedIntegrationEvent(
      domainEvent.Id,
      domainEvent.OccurredOnUtc,
      domainEvent.DraftPartId,
      domainEvent.ImdbId,
      domainEvent.MovieTitle),
      cancellationToken);
  }
}
