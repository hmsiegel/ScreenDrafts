namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules;

internal sealed class CommunityRuleVetoAppliedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<CommunityRuleVetoAppliedDomainEvent>
{
  public override async Task Handle(
    CommunityRuleVetoAppliedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await eventBus.PublishAsync(
      new CommunityRuleAppliedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: dateTimeProvider.UtcNow,
        draftPartPublicId: domainEvent.DraftPartPublicId,
        tmdbId: domainEvent.TmdbId,
        movieTitle: domainEvent.MovieTitle,
        playOrder: domainEvent.PlayOrder,
        boardPosition: domainEvent.BoardPosition,
        ruleKind: domainEvent.RuleKind,
        targetSlot: domainEvent.TargetSlot
      ),
      cancellationToken
    );
  }
}
