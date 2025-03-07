namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;

public static class DrafterErrors
{
  public static readonly SDError RolloverVetoAlreadyExists =
    SDError.Conflict(
      "Drafters.RolloverVetoAlreadyExists",
      "A rollover veto already exists for this drafter.");

  public static readonly SDError RolloverVetoOverrideAlreadyExists =
    SDError.Conflict(
      "Drafters.RolloverVetoOverrideAlreadyExists",
      "A rollover veto override already exists for this drafter.");

  public static SDError NotFound(Guid drafterId) =>
    SDError.NotFound(
      "Drafters.NotFound",
      $"Drafter with ID {drafterId} not found.");

  public static SDError VetoNotFound(Guid vetoId) =>
    SDError.NotFound(
      "Drafters.VetoNotFound",
      $"Veto with ID {vetoId} not found.");

  public static readonly SDError CannotCreatDrafter =
    SDError.Failure(
      "Drafters.CannotCreatDrafter",
      "Cannot create drafter without user ID or name.");

  public static readonly SDError InvalidQuestionsWon =
    SDError.Failure(
      "Drafters.InvalidQuestionsWon",
      "Questions won must be greater than or equal to 0.");

  public static readonly SDError InvalidPosition =
    SDError.Failure(
      "Drafters.InvalidPosition",
      "Position must be greater than or equal to 0 and less than the total number of drafters.");

  public static SDError VetoOverrideNotFound(Guid vetoOverrideId) =>
    SDError.NotFound(
      "Drafters.VetoOverrideNotFound",
      $"Veto override with ID {vetoOverrideId} not found.");
}
