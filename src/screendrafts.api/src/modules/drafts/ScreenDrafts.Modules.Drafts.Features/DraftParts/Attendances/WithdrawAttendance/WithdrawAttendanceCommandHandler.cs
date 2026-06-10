// ═══════════════════════════════════════════════════════════════════════════════
// WithdrawAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/withdraw
// Person or admin withdraws. Any non-Withdrawn status → Withdrawn.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.WithdrawAttendance;

internal sealed class WithdrawAttendanceCommandHandler(
  IDraftPartRepository draftPartRepository,
  IAttendanceRepository attendanceRepository
) : ICommandHandler<WithdrawAttendanceCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IAttendanceRepository _attendanceRepository = attendanceRepository;

  public async Task<Result> Handle(
    WithdrawAttendanceCommand request,
    CancellationToken cancellationToken
  )
  {
    // Allow self-withdrawal or admin (admin uses DraftPartUpdate permission, which
    // bypasses the self-check at the policy level; here we enforce self for AttendanceWithdraw).
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

    var result = attendance.Withdraw();

    if (result.IsFailure)
      return result;

    _attendanceRepository.Update(attendance);

    return Result.Success();
  }
}
