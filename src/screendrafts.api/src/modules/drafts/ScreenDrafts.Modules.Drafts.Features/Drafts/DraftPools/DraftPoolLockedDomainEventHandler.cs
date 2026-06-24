namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools;

internal sealed class DraftPoolLockedDomainEventHandler(
  IDraftBoardRepository draftBoardRepository,
  IDraftRepository draftRepository
) : DomainEventHandler<DraftPoolLockedDomainEvent>
{
  private readonly IDraftBoardRepository _draftBoardRepository = draftBoardRepository;
  private readonly IDraftRepository _draftRepository = draftRepository;

  public override async Task Handle(
    DraftPoolLockedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    var draftId = new DraftId(domainEvent.DraftId);

    var draft = await _draftRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return;
    }

    var boards = await _draftBoardRepository.GetAllByDraftIdAsync(draft.Id, cancellationToken);

    foreach (var board in boards)
    {
      board.Lock();
      _draftBoardRepository.Update(board);
    }
  }
}
