// Suggested location: Features/GuestDrafts/PlayPick/GuestDraftPickPlayedDomainEventHandler.cs
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.PlayPick;

internal sealed class GuestDraftPickPlayedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<PickPlayedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    PickPlayedDomainEvent domainEvent,
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
