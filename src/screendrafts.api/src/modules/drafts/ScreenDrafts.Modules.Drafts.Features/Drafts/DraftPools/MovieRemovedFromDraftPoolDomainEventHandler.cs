namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools;

internal sealed class MovieRemovedFromDraftPoolDomainEventHandler(
  IDraftBoardRepository draftBoardRepository,
  IDraftRepository draftRepository,
  IUnitOfWork unitOfWork
) : DomainEventHandler<MovieRemovedFromDraftPoolDomainEvent>
{
  private readonly IDraftBoardRepository _draftBoardRepository = draftBoardRepository;
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public override async Task Handle(
    MovieRemovedFromDraftPoolDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    var draftId = new DraftId(domainEvent.DraftId);

    var draft = await _draftRepository.GetByIdWithPartsAndParticipantsAsync(
      draftId,
      cancellationToken
    );

    if (draft is null)
    {
      return;
    }

    var boards = await _draftBoardRepository.GetAllByDraftIdAsync(draft.Id, cancellationToken);

    foreach (var board in boards)
    {
      board.SyncRemoveItem(domainEvent.TmdbId);
      _draftBoardRepository.Update(board);
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);
  }
}
