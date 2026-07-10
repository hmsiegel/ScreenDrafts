namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class CommissionerOverrideAppliedDomainEventHandler(
  IEventBus eventBus,
  ICacheService cacheService
) : DomainEventHandler<CommissionerOverrideAppliedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly ICacheService _cacheService = cacheService;

  public override async Task Handle(
    CommissionerOverrideAppliedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _cacheService.RemoveAsync(
      key: DraftsCacheKeys.PickList(domainEvent.DraftPartPublicId),
      cancellationToken: cancellationToken
    );

    await _eventBus.PublishAsync(
      new CommissionerOverrideAppliedIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: domainEvent.DraftPartPublicId,
        tmdbId: domainEvent.TmdbId,
        movieTitle: domainEvent.MovieTitle,
        participantId: domainEvent.ParticipantId,
        participantKind: domainEvent.ParticipantKind,
        boardPosition: domainEvent.BoardPosition,
        playOrder: domainEvent.PlayOrder
      ),
      cancellationToken
    );

    await _eventBus.PublishAsync(
      new PickUnlockedIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: domainEvent.DraftPartPublicId,
        draftId: domainEvent.DraftId,
        draftPublicId: domainEvent.DraftPublicId,
        moviePublicId: domainEvent.MoviePublicId,
        movieTitle: domainEvent.MovieTitle,
        tmdbId: domainEvent.TmdbId,
        boardPosition: domainEvent.BoardPosition,
        playedByParticipantId: domainEvent.ParticipantId,
        playedByParticipantKind: domainEvent.ParticipantKind,
        unlockReason: PickUnlockReason.CommissionerOverride
      ),
      cancellationToken
    );
  }
}
