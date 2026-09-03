// Suggested location: Features/GuestDrafts/PlayPick/GuestDraftPickPlayedDomainEventHandler.cs
namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.PlayPick;

internal sealed class GuestDraftPickPlayedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<GuestDraftPickPlayedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    GuestDraftPickPlayedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      new GuestDraftPickSubmittedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        guestDraftId: domainEvent.GuestDraftId,
        guestDraftPublicId: domainEvent.GuestDraftPublicId,
        pickId: domainEvent.PickId,
        playOrder: domainEvent.PlayOrder,
        boardPosition: domainEvent.BoardPosition,
        moviePublicId: domainEvent.MoviePublicId,
        playedByParticipantId: domainEvent.PlayedByParticipantId,
        actedByPublicId: domainEvent.ActedByPublicId,
        revealAuthorizedParticipantId: domainEvent.RevealAuthorizedParticipantId
      ),
      cancellationToken
    );
  }
}
