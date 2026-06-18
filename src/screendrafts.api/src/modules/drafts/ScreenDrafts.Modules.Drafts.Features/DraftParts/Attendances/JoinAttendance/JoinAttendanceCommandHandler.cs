// ── Patch to JoinAttendanceCommandHandler.cs ──────────────────────────────────
// Replace the CallerPersonPublicId != PersonPublicId check with a user→person
// resolution so callers passing their u_ public ID can join their pe_ record.
// Inject IDbConnectionFactory to resolve the mapping.

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.JoinAttendance;

internal sealed class JoinAttendanceCommandHandler(
  IDraftPartRepository draftPartRepository,
  IAttendanceRepository attendanceRepository,
  IPersonRepository personRepository
) : ICommandHandler<JoinAttendanceCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IAttendanceRepository _attendanceRepository = attendanceRepository;
  private readonly IPersonRepository _personRepository = personRepository;

  public async Task<Result> Handle(
    JoinAttendanceCommand request,
    CancellationToken cancellationToken
  )
  {
    // Fall back to the raw value if no user record found (supports pe_ passed directly).
    var callerPersonPublicId = await _personRepository.GetByUserIdAsync(
      request.UserId,
      cancellationToken
    );

    if (callerPersonPublicId is null)
    {
      return Result.Failure(AttendanceErrors.NotFound(request.UserId));
    }

    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));

    var attendance = await _attendanceRepository.GetByPartAndPersonAsync(
      draftPart.Id,
      callerPersonPublicId.PublicId,
      cancellationToken
    );

    if (attendance is null)
      return Result.Failure(AttendanceErrors.NotFound(callerPersonPublicId.PublicId));

    var result = attendance.Join();

    if (result.IsFailure)
      return result;

    _attendanceRepository.Update(attendance);

    return Result.Success();
  }
}
