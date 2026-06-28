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

    await _unitOfWork.SaveChangesAsync(cancellationToken);
  }
}
