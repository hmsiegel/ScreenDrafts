namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Errors;

public static class BoostersChampionAssignmentErrors
{
  public static readonly SDError CreationFailed = SDError.Problem(
    code: "BoostersChampionAssignment.CreationFailed",
    description: "Failed to create the Booster's Champion assignment."
  );

  public static readonly SDError AssignedDrafterRequired = SDError.Problem(
    code: "BoostersChampionAssignment.AssignedDrafterRequired",
    description: "An assigned drafter is required."
  );

  public static readonly SDError NotModifiable = SDError.Conflict(
    code: "BoostersChampionAssignment.NotModifiable",
    description: "Booster's Champion assignments can only be changed while the draft part is Created."
  );

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      code: "BoostersChampionAssignment.NotFound",
      description: $"Booster's Champion assignment '{publicId}' is not found."
    );

  public static SDError FilmAlreadyAssigned(int tmdbId) =>
    SDError.Conflict(
      code: "BoostersChampionAssignment.FilmAlreadyAssigned",
      description: $"TMDb film '{tmdbId}' is already assigned to another Booster's Champion."
    );
}
