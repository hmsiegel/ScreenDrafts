namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class PickCreatedDomainEventHandler(
  IEventBus eventBus,
  ICacheService cacheService,
  IDraftPartRepository draftPartRepository,
  IDraftPoolRepository draftPoolRepository,
  IDraftBoardRepository draftBoardRepository,
  ICandidateListRepository candidateListRepository,
  ParticipantResolver participantResolver,
  IUnitOfWork unitOfWork,
  IPickRepository pickRepository
) : DomainEventHandler<PickAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPoolRepository _draftPoolRepository = draftPoolRepository;
  private readonly IDraftBoardRepository _draftBoardRepository = draftBoardRepository;
  private readonly ICandidateListRepository _candidateListRepository = candidateListRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;
  private readonly IPickRepository _pickRepository = pickRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public override async Task Handle(
    PickAddedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _cacheService.RemoveAsync(
      DraftsCacheKeys.PickList(domainEvent.DraftPartPublicId),
      cancellationToken
    );

    var draftPart = await _draftPartRepository.GetByIdAsync(
      DraftPartId.Create(domainEvent.DraftPartId),
      cancellationToken
    );

    var pickId = PickId.Create(domainEvent.PickId);

    if (draftPart is not null)
    {
      // Pool, board, and candidate list are independent, optional, and can
      // coexist for the same participant/draft — NOT mutually exclusive.
      // A pool is draft-wide (Super Drafts, exhaustive — every movie must be
      // picked). A board is per-participant (personal ranked/unranked list).
      // A candidate list is draft-part-wide and shared (inexhaustive
      // brainstorm list, large-pool drafts). All three are checked and
      // updated independently below, each guarded against not existing for
      // this draft/participant, which is a normal, expected state.

      var pool = await _draftPoolRepository.GetByDraftIdAsync(draftPart.DraftId, cancellationToken);

      if (pool is not null)
      {
        pool.RemoveMovie(domainEvent.TmdbId!.Value);

        _draftPoolRepository.Update(pool);

        await _cacheService.RemoveAsync(
          DraftsCacheKeys.DraftPool(domainEvent.DraftPublicId),
          cancellationToken
        );
      }

      var participant = await _participantResolver.ResolveByParticpantIdAsync(
        domainEvent.ParticipantId,
        ParticipantKind.FromValue(domainEvent.ParticipantKind),
        cancellationToken
      );

      // Resolution can legitimately fail — e.g. this participant has no
      // draft board for this draft at all, which is a normal, supported
      // state (boards are optional, not every participant creates one).
      // Previously this forced participant!.Value, which threw
      // InvalidOperationException and silently killed the entire domain
      // event handler whenever it happened, the same bug fixed in
      // VetoAppliedDomainEventHandler.
      if (participant is not null)
      {
        var board = await _draftBoardRepository.GetByDraftAndParticipantAsync(
          draftPart.DraftId,
          participant.Value,
          cancellationToken
        );

        if (board is not null)
        {
          board.RemoveItem(domainEvent.TmdbId!.Value);
          _draftBoardRepository.Update(board);

          await _cacheService.RemoveAsync(
            DraftsCacheKeys.DraftBoard(domainEvent.DraftPublicId, domainEvent.ParticipantId),
            cancellationToken
          );
        }
      }

      // Candidate list entries are inexhaustive and shared — picking does
      // not remove the entry, just flags it picked so RestoreToAvailable
      // can cleanly reverse it on veto. Independent of pool/board above:
      // a movie can simultaneously be in the candidate list AND someone's
      // board, for instance.
      if (domainEvent.TmdbId is not null)
      {
        var candidateEntry = await _candidateListRepository.FindByTmdbIdAsync(
          draftPart.Id,
          domainEvent.TmdbId.Value,
          cancellationToken
        );

        if (candidateEntry is not null)
        {
          var markResult = candidateEntry.MarkAsPicked(pickId);

          if (markResult.IsSuccess)
          {
            _candidateListRepository.Update(candidateEntry);
          }
        }
      }

      await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    var pick = await _pickRepository.GetByIdAsync(pickId, cancellationToken);

    if (pick is null || !pick.IsRevealed)
    {
      return;
    }

    await _eventBus.PublishAsync(
      new PickAddedIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: domainEvent.DraftPartPublicId,
        imdbId: domainEvent.ImdbId!,
        movieTitle: domainEvent.MovieTitle,
        tmdbId: domainEvent.TmdbId,
        boardPosition: domainEvent.BoardPosition,
        playOrder: domainEvent.PlayOrder,
        participantId: domainEvent.ParticipantId,
        participantKind: domainEvent.ParticipantKind
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

    await _eventBus.PublishAsync(
      new PickLockedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: DateTime.UtcNow,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: domainEvent.DraftPartPublicId,
        draftId: domainEvent.DraftId,
        draftPublicId: domainEvent.DraftPublicId,
        moviePublicId: domainEvent.MoviePublicId,
        movieTitle: domainEvent.MovieTitle,
        tmdbId: domainEvent.TmdbId,
        boardPosition: domainEvent.BoardPosition,
        participantIdValue: domainEvent.ParticipantId,
        participantKindValue: domainEvent.ParticipantKind,
        canonicalPolicyValue: domainEvent.CanonicalPolicyValue,
        hasMainFeedRelease: hasMainFeedRelease
      ),
      cancellationToken
    );
  }
}
