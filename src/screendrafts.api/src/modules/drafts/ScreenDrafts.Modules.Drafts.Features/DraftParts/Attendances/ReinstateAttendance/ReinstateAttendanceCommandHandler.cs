// ═══════════════════════════════════════════════════════════════════════════════
// ReinstateAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/reinstate
// Admin reinstates a withdrawn record. Withdrawn → Pending.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ReinstateAttendance;

internal sealed class ReinstateAttendanceCommandHandler(
  IDraftPartRepository draftPartRepository,
  IAttendanceRepository attendanceRepository
) : ICommandHandler<ReinstateAttendanceCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IAttendanceRepository _attendanceRepository = attendanceRepository;

  public async Task<Result> Handle(
    ReinstateAttendanceCommand request,
    CancellationToken cancellationToken
  )
  {
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

    var result = attendance.Reinstate();

    if (result.IsFailure)
      return result;

    _attendanceRepository.Update(attendance);

    return Result.Success();
  }
}
