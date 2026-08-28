using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.UpdateDrafterTeamName;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Updates the name of a drafter team.";
    Description =
      "Updates the name of a drafter team. Requires the 'DrafterTeamMembers' permission.";
    Response(StatusCodes.Status204NoContent, "The drafter team name was successfully updated.");
    Response(
      StatusCodes.Status401Unauthorized,
      "Unauthorized. The user must be authenticated to perform this action."
    );
    Response(
      StatusCodes.Status403Forbidden,
      "Forbidden. The user does not have the required permissions to perform this action."
    );
    Response(
      StatusCodes.Status404NotFound,
      "Not Found. The specified drafter team does not exist."
    );
  }
}
