
namespace ScreenDrafts.Modules.Drafts.Infrastructure.GameBoards;

internal sealed class GameBoardRepository(DraftsDbContext dbContext) : IGameBoardRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(GameBoard gameBoard)
  {
    _dbContext.GameBoards.Add(gameBoard);
  }

  public void Update(GameBoard gameBoard)
  {
    _dbContext.GameBoards.Update(gameBoard);
  }

  public async Task<GameBoard> GetByDraftIdAsync(DraftId draftId, CancellationToken cancellationToken = default)
  {
    var gameBoard = await _dbContext.GameBoards
      .Where(g => g.DraftId == draftId)
      .Include(g => g.DraftPositions)
      .FirstOrDefaultAsync(cancellationToken);

    return gameBoard!;
  }

  public async Task<List<DraftPosition>> ListDraftPositionsByGameBoardIdAsync(GameBoardId gameBoardId, CancellationToken cancellationToken = default)
  {
    var draftPositions = await _dbContext.DraftPositions
      .Where(dp => dp.GameBoardId == gameBoardId)
      .ToListAsync(cancellationToken);

    return draftPositions;
  }
}
