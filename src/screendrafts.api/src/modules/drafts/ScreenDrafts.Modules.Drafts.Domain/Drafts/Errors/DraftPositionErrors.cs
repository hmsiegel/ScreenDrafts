namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class DraftPositionErrors
{
  public static readonly SDError NameIsRequired =
    SDError.Failure(
      "DraftPosition.NameIsRequired",
      "The name of the draft position is required.");

  public static readonly SDError PicksAreRequired =
    SDError.Failure(
      "DraftPosition.PicksAreRequired",
      "The picks of the draft position are required.");

  public static readonly SDError DrafterAlreadyAssigned =
    SDError.Failure(
      "DraftPosition.DrafterAlreadyAssigned",
      "This draft position has already been assigned a drafter.");

  public static SDError NotFound(Guid id) =>
    SDError.NotFound(
      "DraftPosition.NotFound",
      $"Draft position with ID {id} was not found.");

  public static readonly SDError DrafterTeamAlreadyAssigned =
    SDError.Failure(
      "DraftPosition.DrafterTeamAlreadyAssigned",
      "This draft position has already been assigned a drafter team.");
}
