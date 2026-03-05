using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GetPickList;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get the pick list for a draft part.";
    Description = "Retrives the list of picks for a specific draft part, including details about each pick such as the movie information and the participant who made the pick.";
    Response<GetPickListResponse>(200, "Success");
    Response(StatusCodes.Status400BadRequest, "Bad Request");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized");
    Response(StatusCodes.Status403Forbidden, "Forbidden");
    Response(StatusCodes.Status404NotFound, "Draft part not found");
  }
}
