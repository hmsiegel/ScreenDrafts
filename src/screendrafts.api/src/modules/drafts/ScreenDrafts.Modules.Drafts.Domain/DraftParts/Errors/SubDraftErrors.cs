namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Errors;

public static class SubDraftErrors
{
  public static readonly SDError SubDraftsOnlyAllowedForSpeedDrafts = SDError.Conflict(
    code: "DraftPart.SubDraftsOnlyAllowedForSpeedDrafts",
    description: "Sub drafts are only allowed for speed drafts."
  );

  public static readonly SDError SubjectNameCannotBeEmpty = SDError.Conflict(
    code: "DraftPart.SubjectNameCannotBeEmpty",
    description: "Subject name cannot be empty."
  );

  public static readonly SDError CannotActivateSubDraft = SDError.Conflict(
    code: "SubDraft.CannotActivate",
    description: "Sub-draft can only be activated from Pending status."
  );

  public static readonly SDError CannotCompleteSubDraft = SDError.Conflict(
    code: "SubDraft.CannotComplete",
    description: "Sub-draft can only be completed from Active status."
  );

  public static readonly SDError SubjectAlreadySet = SDError.Conflict(
    code: "SubDraft.SubjectAlreadySet",
    description: "Subject has already been set for this sub-draft."
  );

  public static readonly SDError MustBeActive = SDError.Conflict(
    code: "SubDraft.MustBeActive",
    description: "Sub-draft must be Active to perform this operation."
  );

  public static readonly SDError AlreadyCompleted = SDError.Conflict(
    code: "SubDraft.AlreadyCompleted",
    description: "Sub-draft is already completed."
  );

  public static readonly SDError NoNextSubDraft = SDError.Conflict(
    code: "SubDraft.NoNextSubDraft",
    description: "There is no next sub-draft to advance to."
  );

  public static readonly SDError GameBoardNotFound = SDError.Conflict(
    code: "SubDraft.GameBoardNotFound",
    description: "Game board not found for this sub-draft."
  );

  public static readonly SDError SpeedDraftRequiresExactlyTwoParticipants = SDError.Conflict(
    code: "SubDraft.SpeedDraftRequiresExactlyTwoParticipants",
    description: "Speed draft requires exactly two participants."
  );

  public static readonly SDError PositionsAlreadySet = SDError.Conflict(
    code: "SubDraft.PositionsAlreadySet",
    description: "Positions have already been set for this sub-draft."
  );

  public static readonly SDError PositionsAlreadyChosen = SDError.Conflict(
    code: "SubDraft.PositionsAlreadyChosen",
    description: "Positions have already been chosen for this sub-draft."
  );

  public static readonly SDError InvalidPositionChoice = SDError.Conflict(
    code: "SubDraft.InvalidPositionChoice",
    description: "Invalid position choice."
  );

  public static readonly SDError SubjectImdbIdCannotBeEmptyForPersonSubjects = SDError.Conflict(
    code: "SubDraft.SubjectImdbIdCannotBeEmptyForPersonSubjects",
    description: "Subject IMDb ID cannot be empty for person subjects."
  );

  public static readonly SDError SubjectImdbNotAllowedForWordSubjects = SDError.Conflict(
    code: "SubDraft.SubjectImdbNotAllowedForWordSubjects",
    description: "Subject IMDb ID is not allowed for word subjects."
  );

  public static readonly SDError AllSubDraftsMustBeCompletedBeforeDraftPartCanComplete =
    SDError.Conflict(
      code: "SubDraft.AllSubDraftsMustBeCompletedBeforeDraftPartCanComplete",
      description: "All sub-drafts must be completed before the draft part can complete."
    );

  public static SDError NotFound(Guid value) =>
    SDError.NotFound(
      code: "SubDraft.NotFound",
      description: $"Sub-draft with ID '{value}' is not found."
    );

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      code: "SubDraft.NotFound",
      description: $"Sub-draft with public Id '{publicId}' is not found."
    );
}
