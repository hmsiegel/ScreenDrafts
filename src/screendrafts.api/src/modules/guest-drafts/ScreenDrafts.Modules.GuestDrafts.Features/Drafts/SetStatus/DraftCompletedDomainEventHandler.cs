namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.SetStatus;

internal sealed class DraftCompletedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<DraftCompletedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    DraftCompletedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      new GuestDraftCompletedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        guestDraftId: domainEvent.DraftId,
        guestDraftPublicId: domainEvent.DraftPublicId,
        totalPicks: domainEvent.TotalPicks,
        vetoCount: domainEvent.VetoCount
      ),
      cancellationToken
    );
  }
}
