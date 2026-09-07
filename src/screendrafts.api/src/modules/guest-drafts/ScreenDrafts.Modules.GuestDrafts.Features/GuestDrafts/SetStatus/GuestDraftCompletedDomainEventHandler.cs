// Suggested location: Features/GuestDrafts/SetGuestDraftStatus/GuestDraftCompletedDomainEventHandler.cs
namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.SetStatus;

internal sealed class GuestDraftCompletedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<GuestDraftCompletedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    GuestDraftCompletedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      new GuestDraftCompletedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        guestDraftId: domainEvent.GuestDraftId,
        guestDraftPublicId: domainEvent.GuestDraftPublicId,
        totalPicks: domainEvent.TotalPicks,
        vetoCount: domainEvent.VetoCount
      ),
      cancellationToken
    );
  }
}
