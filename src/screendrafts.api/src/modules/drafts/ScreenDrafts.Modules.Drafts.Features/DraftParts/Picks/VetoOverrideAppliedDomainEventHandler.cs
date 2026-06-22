namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class VetoOverrideAppliedDomainEventHandler(
  IEventBus eventBus,
  ICacheService cacheService,
  IDraftPartRepository draftPartRepository,
  IDraftPoolRepository poolRepository,
  IDraftBoardRepository boardRepository,
  ParticipantResolver participantResolver,
  IUnitOfWork unitOfWork,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<VetoOverrideAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPoolRepository _poolRepository = poolRepository;
  private readonly IDraftBoardRepository _boardRepository = boardRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    VetoOverrideAddedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _cacheService.RemoveAsync(
      key: DraftsCacheKeys.PickList(draftPartPublicId: domainEvent.DraftPartPublicId),
      cancellationToken: cancellationToken
    );

    var draftPart = await _draftPartRepository.GetByIdAsync(
      draftPartId: DraftPartId.Create(domainEvent.DraftPartId),
      cancellationToken: cancellationToken
    );

    if (draftPart is not null)
    {
      // Pool and board are independent and can coexist — NOT mutually
      // exclusive. See VetoAppliedDomainEventHandler /
      // PickCreatedDomainEventHandler for the same restructuring and
      // rationale.

      var pool = await _poolRepository.GetByDraftIdAsync(draftPart.DraftId, cancellationToken);

      if (pool is not null)
      {
        pool.RemoveMovie(domainEvent.TmdbId);

        _poolRepository.Update(pool);

        await _cacheService.RemoveAsync(
          key: DraftsCacheKeys.DraftPool(domainEvent.DraftPublicId),
          cancellationToken: cancellationToken
        );
      }

      var participant = await _participantResolver.ResolveByParticpantIdAsync(
        participantId: domainEvent.ParticipantId,
        participantKind: ParticipantKind.FromValue(domainEvent.ParticipantKind),
        cancellationToken: cancellationToken
      );

      // Resolution can legitimately fail — e.g. the overriding participant
      // has no draft board for this draft (boards are optional). Previously
      // this forced participant!.Value, which threw
      // InvalidOperationException and silently killed the entire domain
      // event handler — meaning VetoOverrideAppliedIntegrationEvent AND the
      // downstream PickLockedIntegrationEvent (further below) never
      // published, so neither the live override broadcast nor the
      // honorific lock would fire. Treat a failed resolution the same way
      // a missing board is already treated below: nothing to update, skip
      // and continue.
      if (participant is not null)
      {
        var board = await _boardRepository.GetByDraftAndParticipantAsync(
          draftId: draftPart.DraftId,
          participantId: participant.Value,
          cancellationToken: cancellationToken
        );

        if (board is not null)
        {
          board.RemoveItem(domainEvent.TmdbId);

          _boardRepository.Update(board);

          await _cacheService.RemoveAsync(
            key: DraftsCacheKeys.DraftBoard(domainEvent.DraftPublicId, domainEvent.ParticipantId),
            cancellationToken: cancellationToken
          );
        }
      }

      await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    await _eventBus.PublishAsync(
      new VetoOverrideAppliedIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: domainEvent.DraftPartPublicId,
        playOrder: domainEvent.PlayOrder,
        tmdbId: domainEvent.TmdbId,
        movieTitle: domainEvent.MovieTitle!,
        overriddenByParticipantId: domainEvent.ParticipantId,
        overriddenByParticipantKind: domainEvent.ParticipantKind
      ),
      cancellationToken
    );

    if (domainEvent.CanonicalPolicyValue == 1)
    {
      return;
    }

    var hasMainFeedRelease =
      draftPart?.Releases.Any(r => r.ReleaseChannel == ReleaseChannel.MainFeed) ?? false;

    if (domainEvent.CanonicalPolicyValue == 2 && !hasMainFeedRelease)
    {
      return;
    }

    var pick = draftPart!.Picks.FirstOrDefault(p =>
      p.Movie.TmdbId == domainEvent.TmdbId && p.IsActiveOnFinalBoard
    );

    if (pick is null)
    {
      return;
    }

    await _eventBus.PublishAsync(
      new PickLockedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: domainEvent.DraftPartPublicId,
        draftId: domainEvent.DraftId,
        draftPublicId: domainEvent.DraftPublicId,
        moviePublicId: pick.Movie.PublicId,
        movieTitle: pick.Movie.MovieTitle,
        tmdbId: pick.Movie.TmdbId,
        boardPosition: pick.Position,
        participantIdValue: domainEvent.ParticipantId,
        participantKindValue: domainEvent.ParticipantKind,
        canonicalPolicyValue: domainEvent.CanonicalPolicyValue,
        hasMainFeedRelease: hasMainFeedRelease
      ),
      cancellationToken
    );
  }
}
