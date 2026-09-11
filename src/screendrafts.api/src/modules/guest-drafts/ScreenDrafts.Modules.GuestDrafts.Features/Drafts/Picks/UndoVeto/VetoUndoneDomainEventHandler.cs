namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.UndoVeto;

internal sealed class VetoUndoneDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<VetoUndoneDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    VetoUndoneDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      new GuestDraftVetoUndoneIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        guestDraftId: domainEvent.GuestDraftId,
        guestDraftPublicId: domainEvent.GuestDraftPublicId,
        pickId: domainEvent.PickId,
        playOrder: domainEvent.PlayOrder,
        moviePublicId: domainEvent.MoviePublicId,
        refundedToParticipantId: domainEvent.RefundedToParticipantId,
        vetoTokensRemaining: domainEvent.VetoTokensRemaining,
        overrideTokensRemaining: domainEvent.OverrideTokensRemaining
      ),
      cancellationToken
    );
  }
}
