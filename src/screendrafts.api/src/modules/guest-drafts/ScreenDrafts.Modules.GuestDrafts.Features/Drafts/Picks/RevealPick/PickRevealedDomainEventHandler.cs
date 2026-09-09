namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.RevealPick;

internal sealed class PickRevealedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<PickRevealedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    PickRevealedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      new GuestDraftPickRevealedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        guestDraftId: domainEvent.GuestDraftId,
        guestDraftPublicId: domainEvent.GuestDraftPublicId,
        pickId: domainEvent.PickId,
        playOrder: domainEvent.PlayOrder,
        boardPosition: domainEvent.BoardPosition,
        moviePublicId: domainEvent.MoviePublicId,
        playedByParticipantId: domainEvent.PlayedByParticipantId
      ),
      cancellationToken
    );
  }
}
