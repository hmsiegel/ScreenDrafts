namespace ScreenDrafts.Modules.Drafts.Domain.Attendances;

public interface IAttendanceRepository : IRepository<DraftPartAttendance, DraftPartAttendanceId>
{
  Task<DraftPartAttendance?> GetByPartAndPersonAsync(
    DraftPartId draftPartId,
    string personPublicId,
    CancellationToken cancellationToken = default
  );

  Task<List<DraftPartAttendance>> GetByDraftPartAsync(
    DraftPartId draftPartId,
    CancellationToken cancellationToken = default
  );

  Task<bool> ExistsAsync(
    DraftPartId draftPartId,
    string personPublicId,
    CancellationToken cancellationToken = default
  );
}
