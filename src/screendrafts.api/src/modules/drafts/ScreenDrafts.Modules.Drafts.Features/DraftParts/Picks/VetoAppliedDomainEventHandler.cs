namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class VetoAppliedDomainEventHandler(
  IEventBus eventBus,
  ICacheService cacheService,
  IDraftPartRepository draftPartRepository,
  IDraftPoolRepository draftPoolRepository,
  IDraftBoardRepository draftBoardRepository,
  ICandidateListRepository candidateListRepository,
  ParticipantResolver participantResolver,
  IUnitOfWork unitOfWork
) : DomainEventHandler<VetoAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPoolRepository _draftPoolRepository = draftPoolRepository;
  private readonly IDraftBoardRepository _draftBoardRepository = draftBoardRepository;
  private readonly ICandidateListRepository _candidateListRepository = candidateListRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public override async Task Handle(
    VetoAddedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _cacheService.RemoveAsync(
      key: DraftsCacheKeys.PickList(draftPartPublicId: domainEvent.DraftPartPublicId),
      cancellationToken: cancellationToken
    );

    var draftPart = await _draftPartRepository.GetByIdAsync(
      DraftPartId.Create(domainEvent.DraftPartId),
      cancellationToken
    );

    if (draftPart is not null)
    {
      // Pool, board, and candidate list are independent and can coexist —
      // NOT mutually exclusive. See PickCreatedDomainEventHandler for the
      // same restructuring and rationale (this mirrors it on the restore
      // side).

      var pool = await _draftPoolRepository.GetByDraftIdAsync(draftPart.DraftId, cancellationToken);

      if (pool is not null)
      {
        pool.RestoreMovie(domainEvent.TmdbId!.Value);

        _draftPoolRepository.Update(pool);

        await _cacheService.RemoveAsync(
          key: DraftsCacheKeys.DraftPool(draftPublicId: domainEvent.DraftPublicId),
          cancellationToken: cancellationToken
        );
      }

      var participant = await _participantResolver.ResolveByParticpantIdAsync(
        participantId: domainEvent.ParticipantId,
        participantKind: ParticipantKind.FromValue(domainEvent.ParticipantKind),
        cancellationToken: cancellationToken
      );

      // Resolution can legitimately fail — e.g. the vetoing participant
      // has no draft board for this draft (boards are optional; not every
      // participant creates one). Previously this forced participant!.Value,
      // which threw InvalidOperationException and silently killed the
      // entire domain event handler — meaning VetoAppliedIntegrationEvent
      // and PickUnlockedIntegrationEvent never published, so the live veto
      // broadcast never reached any client even though the veto itself had
      // already been persisted successfully. Treat a failed resolution the
      // same way a missing board is already treated below: there's nothing
      // to restore the movie to, so skip restoration and continue on.
      if (participant is not null)
      {
        var board = await _draftBoardRepository.GetByDraftAndParticipantAsync(
          draftPart.DraftId,
          participant.Value,
          cancellationToken
        );

        if (board is not null)
        {
          board.AddItem(domainEvent.TmdbId!.Value, notes: null, priority: null);

          _draftBoardRepository.Update(board);

          await _cacheService.RemoveAsync(
            key: DraftsCacheKeys.DraftBoard(
              draftPublicId: domainEvent.DraftPublicId,
              userId: domainEvent.ParticipantId
            ),
            cancellationToken: cancellationToken
          );
        }
      }

      // Candidate list entries are inexhaustive and shared — they were
      // never removed on pick, just flagged picked (MarkAsPicked). Vetoing
      // the pick reverses that flag so the entry is available to be picked
      // again by anyone. Independent of pool/board above.
      if (domainEvent.TmdbId is not null)
      {
        var candidateEntry = await _candidateListRepository.FindByTmdbIdAsync(
          draftPart.Id,
          domainEvent.TmdbId.Value,
          cancellationToken
        );

        if (candidateEntry is not null)
        {
          var restoreResult = candidateEntry.RestoreToAvailable();

          if (restoreResult.IsSuccess)
          {
            _candidateListRepository.Update(candidateEntry);
          }
        }
      }

      await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    await _eventBus.PublishAsync(
      new VetoAppliedIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: domainEvent.DraftPartPublicId,
        playOrder: domainEvent.PlayOrder,
        tmdbId: domainEvent.TmdbId!.Value,
        movieTitle: domainEvent.MovieTitle!,
        vetoedByParticipantId: domainEvent.ParticipantId,
        vetoedByParticipantKind: domainEvent.ParticipantKind,
        playedByParticipantId: domainEvent.PlayedByParticipantId,
        playedByParticipantKind: domainEvent.PlayedByParticipantKind
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
        movieTitle: domainEvent.MovieTitle!,
        tmdbId: domainEvent.TmdbId!.Value,
        boardPosition: domainEvent.BoardPosition,
        playedByParticipantId: domainEvent.PlayedByParticipantId,
        playedByParticipantKind: domainEvent.PlayedByParticipantKind,
        unlockReason: PickUnlockReason.Vetoed
      ),
      cancellationToken
    );
  }
}
