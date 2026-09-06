namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafterTeams;

public static class GuestDrafterTeamErrors
{
  public static readonly SDError InvalidName = SDError.Problem(
    "GuestDrafterTeams.InvalidName",
    "Team name is required."
  );

  public static readonly SDError InvalidPublicId = SDError.Problem(
    "GuestDrafterTeams.InvalidPublicId",
    "A valid public id is required."
  );

  public static readonly SDError NotEnoughDrafters = SDError.Problem(
    "GuestDrafterTeams.NotEnoughDrafters",
    "A team must have at least one guest drafter."
  );

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      "GuestDrafterTeams.NotFound",
      $"Guest drafter team with public id {publicId} was not found."
    );
}
