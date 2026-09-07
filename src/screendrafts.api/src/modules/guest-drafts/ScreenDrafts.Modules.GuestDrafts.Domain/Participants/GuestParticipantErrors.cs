namespace ScreenDrafts.Modules.GuestDrafts.Domain.Participants;

public static class GuestParticipantErrors
{
  public static readonly SDError EmptyValue = SDError.Problem(
    "GuestParticipants.EmptyValue",
    "Participant id cannot be empty."
  );

  public static readonly SDError InvalidParticipantKind = SDError.Problem(
    "GuestParticipants.InvalidParticipantKind",
    "Invalid participant kind."
  );
}
