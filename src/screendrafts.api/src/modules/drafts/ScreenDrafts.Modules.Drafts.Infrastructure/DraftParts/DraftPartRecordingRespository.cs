namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftParts;

internal sealed class DraftPartRecordingRespository(DraftsDbContext dbContext) : IDraftPartRecordingRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(DraftPartRecording entity)
  {
    _dbContext.Add(entity);
  }

  public Task<int> CountByDraftPartIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken = default)
  {
    return _dbContext.DraftPartRecordings.CountAsync(x => x.DraftPartId == draftPartId, cancellationToken);
  }

  public void Delete(DraftPartRecording entity)
  {
    _dbContext.Remove(entity);
  }

  public async Task<IReadOnlyList<DraftPartRecording>> GetByDraftPartIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.DraftPartRecordings
      .Where(x => x.DraftPartId == draftPartId)
      .ToListAsync(cancellationToken);
  }

  public async Task<DraftPartRecording?> GetByZoomFileIdAsync(string zoomFileId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.DraftPartRecordings
      .FirstOrDefaultAsync(x => x.ZoomFileId == zoomFileId, cancellationToken);
  }

  public void Update(DraftPartRecording entity)
  {
    _dbContext.Update(entity);
  }
}
