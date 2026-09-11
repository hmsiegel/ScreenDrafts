namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.SetStatus;

internal sealed class DraftStartedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<DraftStartedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    DraftStartedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      new GuestDraftStartedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        guestDraftId: domainEvent.DraftId,
        guestDraftPublicId: domainEvent.DraftPublicId,
        participantCount: domainEvent.ParticipantCount
      ),
      cancellationToken
    );
  }
}
