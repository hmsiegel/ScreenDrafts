using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.AddDrafterToTeam;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Adds a drafter to a drafter team.";
    Description = "Adds the specified drafter to the specified drafter team. The drafter and drafter team must both exist, and the drafter must not already be a member of the team.";
    Response(StatusCodes.Status204NoContent, "The drafter was successfully added to the team.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized. The user must be authenticated to perform this action.");
    Response(StatusCodes.Status403Forbidden, "Forbidden. The user does not have permission to perform this action.");
    Response(StatusCodes.Status404NotFound, "The specified drafter or drafter team was not found.");
  }
}
