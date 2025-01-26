namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
public static class PickErrors
{
  public static readonly SDError PickAlreadyExists =
    SDError.Conflict(
      "Drafts.PickAlreadyExists",
      "A pick already exists for this drafter.");

  public static SDError NotFound(Guid pickId) =>
    SDError.NotFound(
      "Picks.NotFound",
      $"Pick with ID {pickId} not found.");
}
