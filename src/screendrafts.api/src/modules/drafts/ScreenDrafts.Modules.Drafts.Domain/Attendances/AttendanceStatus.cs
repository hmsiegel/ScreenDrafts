namespace ScreenDrafts.Modules.Drafts.Domain.Attendances;

public sealed class AttendanceStatus(string name, int value)
  : SmartEnum<AttendanceStatus>(name, value)
{
  public static readonly AttendanceStatus Pending = new(nameof(Pending), 0);
  public static readonly AttendanceStatus Confirmed = new(nameof(Confirmed), 1);
  public static readonly AttendanceStatus Joined = new(nameof(Joined), 2);
  public static readonly AttendanceStatus Withdrawn = new(nameof(Withdrawn), 3);
}
