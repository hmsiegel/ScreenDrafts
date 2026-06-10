// ═══════════════════════════════════════════════════════════════════════════════
// ConfirmAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/confirm
// Admin confirms person is attending. Pending → Confirmed.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ConfirmAttendance;

internal sealed class ConfirmAttendanceCommandHandler(
  IDraftPartRepository draftPartRepository,
  IAttendanceRepository attendanceRepository
) : ICommandHandler<ConfirmAttendanceCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IAttendanceRepository _attendanceRepository = attendanceRepository;

  public async Task<Result> Handle(
    ConfirmAttendanceCommand request,
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

    var result = attendance.Confirm();

    if (result.IsFailure)
      return result;

    _attendanceRepository.Update(attendance);

    return Result.Success();
  }
}
