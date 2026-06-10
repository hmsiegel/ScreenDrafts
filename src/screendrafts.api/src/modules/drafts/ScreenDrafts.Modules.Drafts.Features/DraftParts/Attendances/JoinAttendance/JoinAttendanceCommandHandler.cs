// ═══════════════════════════════════════════════════════════════════════════════
// JoinAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/join
// Person joins the draft part. Confirmed → Joined.
// Caller must be the person identified by personPublicId (or admin/commissioner).
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.JoinAttendance;

internal sealed class JoinAttendanceCommandHandler(
  IDraftPartRepository draftPartRepository,
  IAttendanceRepository attendanceRepository
) : ICommandHandler<JoinAttendanceCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IAttendanceRepository _attendanceRepository = attendanceRepository;

  public async Task<Result> Handle(
    JoinAttendanceCommand request,
    CancellationToken cancellationToken
  )
  {
    // Caller must be joining their own record.
    // Admins bypass this check via the DraftPartUpdate permission on the endpoint;
    // here we enforce it for the self-service permission (AttendanceJoin).
    if (request.CallerPersonPublicId != request.PersonPublicId)
      return Result.Failure(AttendanceErrors.NotParticipant);

    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));

    var attendance = await _attendanceRepository.GetByPartAndPersonAsync(
      draftPart.Id,
      request.PersonPublicId,
      cancellationToken
    );

    if (attendance is null)
      return Result.Failure(AttendanceErrors.NotFound(request.PersonPublicId));

    var result = attendance.Join();

    if (result.IsFailure)
      return result;

    _attendanceRepository.Update(attendance);

    return Result.Success();
  }
}
