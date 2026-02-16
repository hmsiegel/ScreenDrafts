
using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Errors;

public static class DraftPartErrors
{
  public static readonly SDError DraftNotStarted =
    SDError.Conflict(
      code: "DraftPart.DraftNotStarted",
      description: "Draft part has not started."
  );

  public static readonly SDError CommunityPicksNotAllowed = SDError.Conflict(
    code: "DraftPart.CommunityPicksNotAllowed",
    description: "Community picks are not allowed."
  );

  public static readonly SDError CommunityPicksExceeded = SDError.Conflict(
    code: "DraftPart.CommunityPicksExceeded",
    description: "Community picks exceeded the allowed limit."
  );

  public static readonly SDError NoRemainingVetoes = SDError.Conflict(
    code: "DraftPart.NoRemainingVetoes",
    description: "No remaining vetoes available."
  );

  public static readonly SDError VetoOverridesNotAllowed = SDError.Conflict(
    code: "DraftPart.VetoOverridesNotAllowed",
    description: "Veto overrides are not allowed."
  );

  public static readonly SDError NoRemainingVetoOverrides = SDError.Conflict(
    code: "DraftPart.NoRemainingVetoOverrides",
    description: "No remaining veto overrides available."
  );

  public static readonly SDError PartIndexIsOutOfRange = SDError.Conflict(
    code: "DraftPart.PartIndexIsOutOfRange",
    description: "Part index is out of range."
  );

  public static readonly SDError CannotChangePartIndexAfterDraftHasStarted = SDError.Conflict(
    code: "DraftPart.CannotChangePartIndexAfterDraftHasStarted",
    description: "Cannot change part index after draft has started."
  );

  public static readonly SDError DraftCanOnlyBeStartedIfItIsCreated = SDError.Conflict(
    code: "DraftPart.DraftCanOnlyBeStartedIfItIsCreated",
    description: "Draft can only be started if it is created."
  );

  public static readonly SDError CannotStartDraftWithoutAllParticipants = SDError.Conflict(
    code: "DraftPart.CannotStartDraftWithoutAllParticipants",
    description: "Cannot start draft without all participants."
  );

  public static readonly SDError CannotStartDraftWithoutAllHosts = SDError.Conflict(
    code: "DraftPart.CannotStartDraftWithoutAllHosts",
    description: "Cannot start draft without all hosts."
  );

  public static readonly SDError CannotStartADraftWithoutAtLeastTwoParticipants = SDError.Conflict(
    code: "DraftPart.CannotStartADraftWithoutAtLeastTwoParticipants",
    description: "Cannot start a draft without at least two participants."
  );

  public static readonly SDError MinimumPositionMustBeGreaterThanZero = SDError.Conflict(
    code: "DraftPart.MinimumPositionMustBeGreaterThanZero",
    description: "Minimum position must be greater than zero."
  );

  public static readonly SDError MaxPositionIsOutOfRange = SDError.Conflict(
    code: "DraftPart.MaxPositionIsOutOfRange",
    description: "Maximum position is out of range."
  );

  public static SDError ParticipantAlreadyAdded(Guid participantId) =>
    SDError.Conflict(
      code: "DraftPart.ParticipantAlreadyAdded",
      description: $"Participant with ID '{participantId}' is already added to the draft part."
    );

  public static SDError ParticipantNotFound(Guid participantId) =>
    SDError.NotFound(
      code: "DraftPart.ParticipantNotFound",
      description: $"Participant with ID '{participantId}' is not found in the draft part."
    );

  public static SDError PickPositionAlreadyExists(int position) => 
    SDError.Conflict(
      code: "DraftPart.PickPositionAlreadyExists",
      description: $"Pick position '{position}' already exists."
    );

  public static SDError InvalidPickPlayOrder(int playOrder) =>
    SDError.Conflict(
      code: "DraftPart.InvalidPickPlayOrder",
      description: $"Invalid pick play order '{playOrder}'."
    );

  public static SDError MovieAlreadyPickedInThisDraft(Guid id) => 
    SDError.Conflict(
      code: "DraftPart.MovieAlreadyPickedInThisDraft",
      description: $"Movie with ID '{id}' has already been picked in this draft."
    );

  public static SDError PickNotFound(Guid pickId) =>
    SDError.NotFound(
      code: "DraftPart.PickNotFound",
      description: $"Pick with ID '{pickId}' is not found."
    );

  public static SDError VetoNotFound(Guid value) => SDError.NotFound(
      code: "DraftPart.VetoNotFound",
      description: $"Veto with ID '{value}' is not found."
    );

  public static SDError ParticipantDoesNotBelongToThisDraftPart(ParticipantId playedBy) =>
    SDError.Conflict(
      code: "DraftPart.ParticipantDoesNotBelongToThisDraftPart",
      description: $"Participant with ID '{playedBy}' does not belong to this draft part."
    );

  public static SDError ParticipantAlreadyHasTriviaResult(ParticipantId participantId) => SDError.Conflict(
      code: "DraftPart.ParticipantAlreadyHasTriviaResult",
      description: $"Participant with ID '{participantId}' already has a trivia result."
    );

}
