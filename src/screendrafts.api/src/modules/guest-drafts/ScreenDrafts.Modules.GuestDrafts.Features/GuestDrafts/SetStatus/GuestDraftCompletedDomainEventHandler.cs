// Suggested location: Features/GuestDrafts/SetGuestDraftStatus/GuestDraftCompletedDomainEventHandler.cs
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.SetStatus;

internal sealed class GuestDraftCompletedDomainEventHandler(
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
