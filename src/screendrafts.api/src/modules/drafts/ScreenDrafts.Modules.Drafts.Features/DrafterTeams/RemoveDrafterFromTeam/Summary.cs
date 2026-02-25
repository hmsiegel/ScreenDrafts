using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.RemoveDrafterFromTeam;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Removes a drafter from a drafter team.";
    Description = "Removes a drafter from a drafter team. Requires the 'DrafterTeamMembers' permission.";
    Response(StatusCodes.Status204NoContent, "The drafter was successfully removed from the team.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized. The user must be authenticated to perform this action.");
    Response(StatusCodes.Status403Forbidden, "Forbidden. The user does not have the required permissions to perform this action.");
    Response(StatusCodes.Status404NotFound, "Not Found. The specified drafter or drafter team does not exist.");
  }
}
