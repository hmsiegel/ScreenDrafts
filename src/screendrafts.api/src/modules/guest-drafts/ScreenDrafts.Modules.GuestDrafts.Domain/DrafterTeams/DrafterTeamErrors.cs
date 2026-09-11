namespace ScreenDrafts.Modules.GuestDrafts.Domain.DrafterTeams;

public static class DrafterTeamErrors
{
  public static readonly SDError InvalidName = SDError.Problem(
    "DrafterTeams.InvalidName",
    "Team name is required."
  );

  public static readonly SDError InvalidPublicId = SDError.Problem(
    "DrafterTeams.InvalidPublicId",
    "A valid public id is required."
  );

  public static readonly SDError NotEnoughDrafters = SDError.Problem(
    "DrafterTeams.NotEnoughDrafters",
    "A team must have at least one guest drafter."
  );

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      "DrafterTeams.NotFound",
      $"Drafter team with public id {publicId} was not found."
    );
}
