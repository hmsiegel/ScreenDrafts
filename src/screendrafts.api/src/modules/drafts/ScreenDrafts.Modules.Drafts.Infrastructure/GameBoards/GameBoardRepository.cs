namespace ScreenDrafts.Modules.Drafts.Infrastructure.GameBoards;

internal sealed class GameBoardRepository(DraftsDbContext dbContext) : IGameBoardRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(GameBoard gameBoard)
  {
    _dbContext.GameBoards.Add(gameBoard);
  }

  public async Task<GameBoard> GetByDraftIdAsync(Guid draftId)
  {
    var gameBoard = await _dbContext.GameBoards
      .SingleOrDefaultAsync(g => g.Draft.Id.Value == draftId);

    return gameBoard!;
  }
}
