namespace ScreenDrafts.Modules.GuestDrafts.Domain.Participants;

public static class ParticipantErrors
{
  public static readonly SDError EmptyValue = SDError.Problem(
    "Participants.EmptyValue",
    "Participant id cannot be empty."
  );

  public static readonly SDError InvalidParticipantKind = SDError.Problem(
    "Participants.InvalidParticipantKind",
    "Invalid participant kind."
  );
}
