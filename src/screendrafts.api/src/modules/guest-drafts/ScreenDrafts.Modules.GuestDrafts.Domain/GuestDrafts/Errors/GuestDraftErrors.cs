namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Errors;

public static class GuestDraftErrors
{
  // ── GuestDraft / participants ────────────────────────────────────────────

  public static readonly SDError TitleIsRequired = SDError.Problem(
    "GuestDrafts.TitleIsRequired",
    "Guest draft title is required."
  );

  public static readonly SDError CannotInviteAfterStart = SDError.Problem(
    "GuestDrafts.CannotInviteAfterStart",
    "Participants cannot be invited after the guest draft has started."
  );

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      "GuestDrafts.NotFound",
      $"Guest draft with public id {publicId} was not found."
    );

  public static SDError NotFound(Guid id) =>
    SDError.NotFound("GuestDrafts.NotFound", $"Guest draft with id {id} was not found.");

  public static SDError InvalidType(string type) =>
    SDError.Problem("GuestDrafts.InvalidType", $"'{type}' is not a valid guest draft type.");

  public static SDError ParticipantAlreadyAdded(Guid userId) =>
    SDError.Conflict(
      "GuestDrafts.ParticipantAlreadyAdded",
      $"User with id {userId} has already been added to this guest draft."
    );

  public static SDError ParticipantNotFound(Guid participantId) =>
    SDError.NotFound(
      "GuestDrafts.ParticipantNotFound",
      $"Participant with id {participantId} was not found in this guest draft."
    );

  public static SDError ParticipantNotFound(string participantPublicId) =>
    SDError.NotFound(
      "GuestDrafts.ParticipantNotFound",
      $"Participant with public id {participantPublicId} was not found in this guest draft."
    );

  // ── Board / positions ─────────────────────────────────────────────────────

  public static readonly SDError CannotChangeBoardAfterStart = SDError.Problem(
    "GuestDrafts.CannotChangeBoardAfterStart",
    "The board layout cannot be changed after the guest draft has started."
  );

  public static SDError DraftTypeDoesNotHaveAFixedLayout(string typeName) =>
    SDError.Problem(
      "GuestDrafts.DraftTypeDoesNotHaveAFixedLayout",
      $"'{typeName}' does not have a fixed board layout; use a custom layout instead."
    );

  public static SDError DraftTypeHasAFixedLayout(string typeName) =>
    SDError.Problem(
      "GuestDrafts.DraftTypeHasAFixedLayout",
      $"'{typeName}' has a fixed board layout and cannot be customized."
    );

  public static readonly SDError PositionNameIsRequired = SDError.Problem(
    "GuestDrafts.PositionNameIsRequired",
    "Position name is required."
  );

  public static readonly SDError PositionPicksAreRequired = SDError.Problem(
    "GuestDrafts.PositionPicksAreRequired",
    "Each position must have at least one pick slot."
  );

  public static readonly SDError PositionCreationFailed = SDError.Problem(
    "GuestDrafts.PositionCreationFailed",
    "Position could not be created."
  );

  public static readonly SDError PositionAlreadyAssigned = SDError.Conflict(
    "GuestDrafts.PositionAlreadyAssigned",
    "This position has already been assigned to a participant."
  );

  public static readonly SDError PositionDoesNotBelongToThisBoard = SDError.Problem(
    "GuestDrafts.PositionDoesNotBelongToThisBoard",
    "This position does not belong to this guest draft's board."
  );

  public static readonly SDError InvalidNumberOfPositions = SDError.Problem(
    "GuestDrafts.InvalidNumberOfPositions",
    "The number of positions must match the number of participants."
  );

  public static readonly SDError DuplicatePickSlots = SDError.Problem(
    "GuestDrafts.DuplicatePickSlots",
    "Pick slot numbers must not be duplicated across positions."
  );

  // ── Lifecycle ─────────────────────────────────────────────────────────────

  public static readonly SDError DraftCanOnlyBeStartedIfCreated = SDError.Problem(
    "GuestDrafts.DraftCanOnlyBeStartedIfCreated",
    "The guest draft can only be started while it is in the Created state."
  );

  public static readonly SDError CannotStartWithoutAtLeastTwoParticipants = SDError.Problem(
    "GuestDrafts.CannotStartWithoutAtLeastTwoParticipants",
    "A guest draft needs at least two participants to start."
  );

  public static readonly SDError BoardMustBeFullySetUpBeforeStarting = SDError.Problem(
    "GuestDrafts.BoardMustBeFullySetUpBeforeStarting",
    "The board must have one position per participant before the draft can start."
  );

  public static readonly SDError AllPositionsMustBeAssignedBeforeStarting = SDError.Problem(
    "GuestDrafts.AllPositionsMustBeAssignedBeforeStarting",
    "Every position must be assigned to a participant before the draft can start."
  );

  public static readonly SDError DraftNotStarted = SDError.Problem(
    "GuestDrafts.DraftNotStarted",
    "The guest draft has not started."
  );

  public static readonly SDError CannotCompleteIfNotInProgress = SDError.Problem(
    "GuestDrafts.CannotCompleteIfNotInProgress",
    "The guest draft can only be completed while it is in progress."
  );

  public static readonly SDError CannotCompleteWithoutAllPicks = SDError.Problem(
    "GuestDrafts.CannotCompleteWithoutAllPicks",
    "The guest draft cannot be completed until every board position has landed."
  );

  // ── Picks ─────────────────────────────────────────────────────────────────

  public static readonly SDError MovieMustBeProvided = SDError.Problem(
    "GuestDrafts.MovieMustBeProvided",
    "A movie must be provided for the pick."
  );

  public static readonly SDError MovieAlreadyPicked = SDError.Conflict(
    "GuestDrafts.MovieAlreadyPicked",
    "This movie has already been picked in this guest draft."
  );

  public static readonly SDError PickPositionIsOutOfRange = SDError.Problem(
    "GuestDrafts.PickPositionIsOutOfRange",
    "Pick position must be a positive number."
  );

  public static readonly SDError InvalidPlayOrder = SDError.Problem(
    "GuestDrafts.InvalidPlayOrder",
    "Play order must be a positive number."
  );

  public static SDError PickPositionAlreadyExists(int position) =>
    SDError.Conflict(
      "GuestDrafts.PickPositionAlreadyExists",
      $"A pick has already landed at board position {position}."
    );

  public static SDError PickNotFound(Guid pickId) =>
    SDError.NotFound("GuestDrafts.PickNotFound", $"Pick with id {pickId} was not found.");

  public static readonly SDError PickAlreadyVetoed = SDError.Conflict(
    "GuestDrafts.PickAlreadyVetoed",
    "This pick has already been vetoed."
  );

  public static readonly SDError PickAlreadyRevealed = SDError.Conflict(
    "GuestDrafts.PickAlreadyRevealed",
    "This pick has already been revealed."
  );

  public static readonly SDError PickNotVetoed = SDError.Problem(
    "GuestDrafts.PickNotVetoed",
    "This pick has not been vetoed."
  );

  public static readonly SDError CannotUndoVetoThatHasBeenOverridden = SDError.Problem(
    "GuestDrafts.CannotUndoVetoThatHasBeenOverridden",
    "A veto that has already been overridden cannot be undone directly."
  );

  public static readonly SDError CommissionerOverrideAlreadyApplied = SDError.Conflict(
    "GuestDrafts.CommissionerOverrideAlreadyApplied",
    "A commissioner override has already been applied to this pick."
  );

  public static readonly SDError PickRequiredForOverride = SDError.Problem(
    "GuestDrafts.PickRequiredForOverride",
    "A pick must be provided for the commissioner override."
  );

  // ── Vetoes / overrides ────────────────────────────────────────────────────

  public static readonly SDError VetoNotOnMostRecentPick = SDError.Problem(
    "GuestDrafts.VetoNotOnMostRecentPick",
    "A veto may only be applied to the most recently played pick."
  );

  public static readonly SDError NoRemainingVetoes = SDError.Problem(
    "GuestDrafts.NoRemainingVetoes",
    "This participant has no remaining vetoes."
  );

  public static SDError VetoNotFound(Guid pickId) =>
    SDError.NotFound("GuestDrafts.VetoNotFound", $"No active veto was found for pick {pickId}.");

  public static SDError MovieNotFound(string moviePublicId) =>
    SDError.NotFound(
      "GuestDrafts.MovieNotFound",
      $"Movie with public ID {moviePublicId} was not found."
    );

  public static SDError PickNotFoundByPlayOrder(int playOrder) =>
    SDError.NotFound(
      "GuestDrafts.PickNotFoundByPlayOrder",
      $"Pick with play order {playOrder} was not found."
    );

  public static readonly SDError VetoOverrideAlreadyUsed = SDError.Conflict(
    "GuestDrafts.VetoOverrideAlreadyUsed",
    "This veto has already been overridden."
  );

  public static readonly SDError VetoOverridesNotAllowedForThisDraftType = SDError.Problem(
    "GuestDrafts.VetoOverridesNotAllowedForThisDraftType",
    "Veto overrides are not allowed for this guest draft type."
  );

  public static readonly SDError CannotOverrideOwnPick = SDError.Problem(
    "GuestDrafts.CannotOverrideOwnPick",
    "A participant cannot override the veto on their own pick."
  );

  public static readonly SDError NoRemainingVetoOverrides = SDError.Problem(
    "GuestDrafts.NoRemainingVetoOverrides",
    "This participant has no remaining veto overrides."
  );
  public static readonly SDError OnlyOwnerCanPerformThisAction = SDError.Problem(
    "GuestDrafts.OnlyOwnerCanPerformThisAction",
    "Only the guest draft's owner can perform this action."
  );

  public static readonly SDError CallerNotAParticipant = SDError.Problem(
    "GuestDrafts.CallerNotAParticipant",
    "You are not a participant in this guest draft."
  );

  public static readonly SDError InvalidStatusAction = SDError.Problem(
    "GuestDrafts.InvalidStatusAction",
    "Invalid status action."
  );

  public static readonly SDError NotRevealAuthorized = SDError.Problem(
    "GuestDrafts.NotRevealAuthorized",
    "You are not authorized to reveal this pick."
  );
}
