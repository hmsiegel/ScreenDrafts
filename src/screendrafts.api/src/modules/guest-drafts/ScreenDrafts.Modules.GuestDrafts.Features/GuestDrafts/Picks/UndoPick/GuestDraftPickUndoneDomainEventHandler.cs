// Suggested location: Features/GuestDrafts/UndoPick/GuestDraftPickUndoneDomainEventHandler.cs
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.UndoPick;

internal sealed class GuestDraftPickUndoneDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<PickUndoneDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    PickUndoneDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      new GuestDraftPickUndoneIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        guestDraftId: domainEvent.GuestDraftId,
        guestDraftPublicId: domainEvent.GuestDraftPublicId,
        playOrder: domainEvent.PlayOrder,
        boardPosition: domainEvent.BoardPosition,
        moviePublicId: domainEvent.MoviePublicId
      ),
      cancellationToken
    );
  }
}
