// Suggested location: Features/GuestDrafts/SetGuestDraftStatus/GuestDraftStartedDomainEventHandler.cs
namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.SetStatus;

internal sealed class GuestDraftStartedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<GuestDraftStartedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    GuestDraftStartedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      new GuestDraftStartedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        guestDraftId: domainEvent.GuestDraftId,
        guestDraftPublicId: domainEvent.GuestDraftPublicId,
        participantCount: domainEvent.ParticipantCount
      ),
      cancellationToken
    );
  }
}
