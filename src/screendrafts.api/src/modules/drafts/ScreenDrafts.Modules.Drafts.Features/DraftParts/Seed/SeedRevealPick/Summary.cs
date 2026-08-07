using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Seed.SeedRevealPick;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Mocks the reveal of a pick for seeding.";
    Description =
      "Mocks revealing  a submitted pick, for seeding purposes. Only an administrator may call this endpoint.";
    Response(StatusCodes.Status204NoContent, "The pick was revealed successfully.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to reveal picks.");
    Response(StatusCodes.Status404NotFound, "The specified draft part or pick was not found.");
  }
}
