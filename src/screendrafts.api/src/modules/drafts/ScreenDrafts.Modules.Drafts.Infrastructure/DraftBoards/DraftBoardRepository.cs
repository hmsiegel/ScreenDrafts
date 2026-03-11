namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftBoards;

internal sealed class DraftBoardRepository(DraftsDbContext dbContext) : IDraftBoardRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(DraftBoard entity)
  {
    _dbContext.Add(entity);
  }

  public void Delete(DraftBoard entity)
  {
    _dbContext.Remove(entity);
  }

  public Task<bool> ExistsAsync(DraftBoardId id, CancellationToken cancellationToken)
  {
    return _dbContext.DraftBoards.AnyAsync(x => x.Id == id, cancellationToken);
  }

  public Task<List<DraftBoard>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.DraftBoards.ToListAsync(cancellationToken);
  }

  public Task<DraftBoard?> GetByDraftAndParticipantAsync(DraftId draftId, Participant participantId, CancellationToken cancellationToken)
  {
    return _dbContext.DraftBoards.FirstOrDefaultAsync(x => x.DraftId == draftId && x.Participant == participantId, cancellationToken);
  }

  public Task<DraftBoard?> GetByIdAsync(DraftBoardId id, CancellationToken cancellationToken)
  {
    return _dbContext.DraftBoards.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
  }

  public Task<DraftBoard?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext.DraftBoards.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
  }

  public void Update(DraftBoard entity)
  {
    _dbContext.Update(entity);
  }
}
