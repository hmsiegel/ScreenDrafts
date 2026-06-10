// ═══════════════════════════════════════════════════════════════════════════════
// AddAttendance — POST /draft-parts/{draftPartId}/attendances
// Admin adds a person (drafter, host, or commissioner). Creates Pending record.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.AddAttendance;

internal sealed class AddAttendanceCommandHandler(
  IDraftPartRepository draftPartRepository,
  IAttendanceRepository attendanceRepository,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<AddAttendanceCommand, AddAttendanceResponse>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IAttendanceRepository _attendanceRepository = attendanceRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<AddAttendanceResponse>> Handle(
    AddAttendanceCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
      return Result.Failure<AddAttendanceResponse>(DraftPartErrors.NotFound(request.DraftPartId));

    var exists = await _attendanceRepository.ExistsAsync(
      draftPart.Id,
      request.PersonPublicId,
      cancellationToken
    );

    if (exists)
      return Result.Failure<AddAttendanceResponse>(
        AttendanceErrors.AlreadyExists(request.PersonPublicId)
      );

    var createResult = DraftPartAttendance.Create(
      draftPartId: draftPart.Id,
      personPublicId: request.PersonPublicId,
      publicId: _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Attendance)
    );

    if (createResult.IsFailure)
      return Result.Failure<AddAttendanceResponse>(createResult.Errors);

    var attendance = createResult.Value;
    _attendanceRepository.Add(attendance);

    return Result.Success(
      new AddAttendanceResponse
      {
        PublicId = attendance.PublicId,
        PersonPublicId = attendance.PersonPublicId,
        Status = attendance.Status.Name,
      }
    );
  }
}
