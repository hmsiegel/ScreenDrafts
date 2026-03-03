using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Get;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get a drafter team by its public ID.";
    Description = "Gets a drafter team by its public ID. Returns 404 if the drafter team is not found.";
    Response<GetDrafterTeamResponse>(StatusCodes.Status200OK, "The drafter team was found and returned successfully.");
    Response(StatusCodes.Status404NotFound, "The drafter team was not found.", contentType: "application/json");
  }
}
