﻿namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface IGameBoardRepository : IRepository
{
  void Add(GameBoard gameBoard);

  void Update(GameBoard gameBoard);

  Task<GameBoard> GetByDraftIdAsync(DraftId draftId, CancellationToken cancellationToken = default);

  Task<GameBoard> GetByGameBoardId(GameBoardId gameBoardId, CancellationToken cancellationToken = default);

  Task<List<DraftPosition>> ListDraftPositionsByGameBoardIdAsync(GameBoardId gameBoardId, CancellationToken cancellationToken = default);
}
