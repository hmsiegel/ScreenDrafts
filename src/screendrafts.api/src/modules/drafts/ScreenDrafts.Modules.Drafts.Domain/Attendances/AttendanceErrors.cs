namespace ScreenDrafts.Modules.Drafts.Domain.Attendances;

public static class AttendanceErrors
{
  public static SDError NotFound(string personPublicId) =>
    SDError.NotFound(
      "Attendance.NotFound",
      $"No attendance record found for person '{personPublicId}'."
    );

  public static SDError AlreadyExists(string personPublicId) =>
    SDError.Conflict(
      "Attendance.AlreadyExists",
      $"An attendance record already exists for person '{personPublicId}'."
    );

  public static readonly SDError NotPending = SDError.Problem(
    "Attendance.NotPending",
    "Attendance record must be Pending to confirm."
  );

  public static readonly SDError NotConfirmed = SDError.Problem(
    "Attendance.NotConfirmed",
    "Attendance record must be Confirmed before joining."
  );

  public static readonly SDError AlreadyWithdrawn = SDError.Problem(
    "Attendance.AlreadyWithdrawn",
    "Attendance record is already withdrawn."
  );

  public static readonly SDError NotWithdrawn = SDError.Problem(
    "Attendance.NotWithdrawn",
    "Attendance record must be Withdrawn to reinstate."
  );

  public static readonly SDError NotParticipant = SDError.Problem(
    "Attendance.NotParticipant",
    "Person is not a participant or host on this draft part."
  );

  public static readonly SDError InvalidPublicId = SDError.Problem(
    "Attendance.InvalidPublicId",
    "Public ID must not be empty."
  );
}
