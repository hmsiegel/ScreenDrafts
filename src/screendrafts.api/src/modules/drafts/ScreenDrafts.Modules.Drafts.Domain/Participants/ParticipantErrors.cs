namespace ScreenDrafts.Modules.Drafts.Domain.Participants;

public static class ParticipantErrors
{
  public static readonly SDError EmptyValue = SDError.Failure(
    code: "Participant.EmptyValue",
    description: "Participant ID value cannot be empty."
  );

  public static SDError InvalidParticipantKind { get; internal set; } = SDError.Failure(
    code: "Participant.InvalidParticipantKind",
    description: "Invalid participant kind.");

  public static SDError UnknownCommunityParticipant { get; internal set; } = SDError.Failure(
    code: "Participant.UnknownCommunityParticipant",
    description: "Unknown community participant.");

  public static SDError CommunityParticipantIdMismatch { get; internal set; } = SDError.Failure(
    code: "Participant.CommunityParticipantIdMismatch",
    description: "Community participant ID mismatch.");
}
