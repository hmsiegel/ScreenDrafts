// Suggested location: Features/GuestDrafts/ApplyCommissionerOverride/GuestDraftCommissionerOverrideAppliedDomainEventHandler.cs
namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyCommissionerOverride;

internal sealed class GuestDraftCommissionerOverrideAppliedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<GuestDraftCommissionerOverrideAppliedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    GuestDraftCommissionerOverrideAppliedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      new GuestDraftCommissionerOverrideAppliedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        guestDraftId: domainEvent.GuestDraftId,
        guestDraftPublicId: domainEvent.GuestDraftPublicId,
        pickId: domainEvent.PickId,
        playOrder: domainEvent.PlayOrder,
        boardPosition: domainEvent.BoardPosition,
        moviePublicId: domainEvent.MoviePublicId,
        playedByParticipantId: domainEvent.PlayedByParticipantId,
        vetoTokensRemaining: domainEvent.VetoTokensRemaining,
        overrideTokensRemaining: domainEvent.OverrideTokensRemaining
      ),
      cancellationToken
    );
  }
}
