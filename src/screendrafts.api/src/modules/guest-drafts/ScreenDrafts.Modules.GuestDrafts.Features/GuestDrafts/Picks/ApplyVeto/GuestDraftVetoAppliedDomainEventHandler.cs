// Suggested location: Features/GuestDrafts/ApplyVeto/GuestDraftVetoAppliedDomainEventHandler.cs
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyVeto;

internal sealed class GuestDraftVetoAppliedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<VetoAppliedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    VetoAppliedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      new GuestDraftVetoAppliedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        guestDraftId: domainEvent.GuestDraftId,
        guestDraftPublicId: domainEvent.GuestDraftPublicId,
        pickId: domainEvent.PickId,
        playOrder: domainEvent.PlayOrder,
        moviePublicId: domainEvent.MoviePublicId,
        vetoedByParticipantId: domainEvent.VetoedByParticipantId,
        playedByParticipantId: domainEvent.PlayedByParticipantId,
        vetoTokensRemaining: domainEvent.VetoTokensRemaining,
        overrideTokensRemaining: domainEvent.OverrideTokensRemaining
      ),
      cancellationToken
    );
  }
}
