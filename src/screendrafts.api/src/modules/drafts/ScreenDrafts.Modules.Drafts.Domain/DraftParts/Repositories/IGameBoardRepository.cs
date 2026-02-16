namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;
public interface IGameBoardRepository : IRepository
{
  void Add(GameBoard gameBoard);

  void Update(GameBoard gameBoard);

  Task<GameBoard> GetByGameBoardId(GameBoardId gameBoardId, CancellationToken cancellationToken = default);

  Task<List<DraftPosition>> ListDraftPositionsByGameBoardIdAsync(GameBoardId gameBoardId, CancellationToken cancellationToken = default);
}
