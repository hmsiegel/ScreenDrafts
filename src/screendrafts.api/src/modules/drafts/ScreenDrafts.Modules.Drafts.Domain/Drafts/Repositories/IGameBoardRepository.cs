namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface IGameBoardRepository
{
  void Add(GameBoard gameBoard);

  Task<GameBoard> GetByDraftIdAsync(DraftId draftId);
}
