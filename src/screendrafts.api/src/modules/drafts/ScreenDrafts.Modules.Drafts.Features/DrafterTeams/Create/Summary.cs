using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Create;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Creates a new drafter team.";
    Description = "Creates a new drafter team with the specified name.";
    Response<string>(StatusCodes.Status201Created, "The drafter team was created successfully.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to create a drafter team.");
  }
}
