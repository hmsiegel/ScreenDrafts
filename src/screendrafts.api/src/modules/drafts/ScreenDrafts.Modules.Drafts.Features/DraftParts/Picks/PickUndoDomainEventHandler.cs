namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks;

internal sealed class PickUndoDomainEventHandler(
  IEventBus eventBus,
  IDraftPoolRepository draftPoolRepository,
  IDraftBoardRepository boardRepository,
  IUnitOfWork unitOfWork
) : DomainEventHandler<PickUndoDomainEvent>
{
  private readonly IDraftPoolRepository _draftPoolRepository = draftPoolRepository;
  private readonly IDraftBoardRepository _boardRepository = boardRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(
    PickUndoDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    var draftId = DraftId.Create(domainEvent.DraftId);

    var pool = await _draftPoolRepository.GetByDraftIdAsync(draftId, cancellationToken);

    if (pool is not null)
    {
      pool.RestoreMovie(domainEvent.TmdbId);
      _draftPoolRepository.Update(pool);
    }

    var boards = await _boardRepository.GetAllByDraftIdAsync(draftId, cancellationToken);

    foreach (var board in boards)
    {
      board.SyncAddItem(domainEvent.TmdbId);
      _boardRepository.Update(board);
    }

    await _eventBus.PublishAsync(
      new PickUndoneIntegrationEvent(
        id: domainEvent.Id,
        occurredOnUtc: domainEvent.OccurredOnUtc,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: domainEvent.DraftPartPublicId,
        playOrder: domainEvent.PlayOrder,
        boardPosition: domainEvent.BoardPosition,
        tmdbId: domainEvent.TmdbId,
        movieTitle: domainEvent.MovieTitle
      ),
      cancellationToken
    );

    // Reporting doesn't listen for PickUndoneIntegrationEvent — RealTimeUpdates does, for the
    // live undo banner. Reporting's RevertMovieHonorificCommandHandler is wired to
    // PickUnlockedIntegrationEvent (same event veto and commissioner-override already publish),
    // so undo has to publish it too or the movie's canonical-pick row never gets deleted and
    // the honorific never recomputes on undo.
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
        playedByParticipantId: domainEvent.PlayedByParticipantId,
        playedByParticipantKind: domainEvent.PlayedByParticipantKind,
        unlockReason: PickUnlockReason.Undone
      ),
      cancellationToken
    );

    await _unitOfWork.SaveChangesAsync(cancellationToken);
  }
}
