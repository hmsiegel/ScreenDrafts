namespace ScreenDrafts.Modules.Drafts.Infrastructure.Attendances;

internal sealed class AttendanceRepository(DraftsDbContext dbContext) : IAttendanceRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(DraftPartAttendance attendance) => _dbContext.Attendances.Add(attendance);

  public void Update(DraftPartAttendance attendance) => _dbContext.Attendances.Update(attendance);

  public void Delete(DraftPartAttendance attendance) => _dbContext.Attendances.Remove(attendance);

  public Task<DraftPartAttendance?> GetByPartAndPersonAsync(
    DraftPartId draftPartId,
    string personPublicId,
    CancellationToken cancellationToken = default
  ) =>
    _dbContext.Attendances.FirstOrDefaultAsync(
      a => a.DraftPartId == draftPartId && a.PersonPublicId == personPublicId,
      cancellationToken
    );

  public Task<DraftPartAttendance?> GetByPublicIdAsync(
    string publicId,
    CancellationToken cancellationToken
  ) => _dbContext.Attendances.FirstOrDefaultAsync(a => a.PublicId == publicId, cancellationToken);

  public Task<List<DraftPartAttendance>> GetByDraftPartAsync(
    DraftPartId draftPartId,
    CancellationToken cancellationToken = default
  ) =>
    _dbContext.Attendances.Where(a => a.DraftPartId == draftPartId).ToListAsync(cancellationToken);

  public Task<bool> ExistsAsync(
    DraftPartId draftPartId,
    string personPublicId,
    CancellationToken cancellationToken = default
  ) =>
    _dbContext.Attendances.AnyAsync(
      a => a.DraftPartId == draftPartId && a.PersonPublicId == personPublicId,
      cancellationToken
    );

  public async Task<DraftPartAttendance?> GetByIdAsync(
    DraftPartAttendanceId id,
    CancellationToken cancellationToken
  )
  {
    return await _dbContext.Attendances.FindAsync(new object[] { id.Value }, cancellationToken);
  }

  public async Task<bool> ExistsAsync(DraftPartAttendanceId id, CancellationToken cancellationToken)
  {
    return await _dbContext.Attendances.AnyAsync(a => a.Id == id, cancellationToken);
  }

  public async Task<List<DraftPartAttendance>> GetAllAsync(CancellationToken cancellationToken)
  {
    return await _dbContext.Attendances.ToListAsync(cancellationToken);
  }
}
