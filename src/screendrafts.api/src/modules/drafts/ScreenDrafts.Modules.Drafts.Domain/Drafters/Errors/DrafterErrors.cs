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

  public static readonly SDError CannotCreatDrafter =
    SDError.Failure(
      "Drafters.CannotCreatDrafter",
      "Cannot create drafter without user ID or name.");
}
