namespace ScreenDrafts.Modules.Drafts.Domain.Attendances;

public sealed class AttendanceJoinedDomainEvent(
  Guid attendanceId,
  Guid draftPartId,
  string personPublicId
) : DomainEvent
{
  public Guid AttendanceId { get; init; } = attendanceId;
  public Guid DraftPartId { get; init; } = draftPartId;
  public string PersonPublicId { get; init; } = personPublicId;
}
