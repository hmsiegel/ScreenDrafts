using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraftPart;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Creates a new draft part for a given draft.";
    Description = "Creates a new draft part for a given draft.";
    Response<string>(StatusCodes.Status200OK, "The ID of the created draft part.");
    Response(StatusCodes.Status400BadRequest, "If the request is invalid.");
    Response(StatusCodes.Status401Unauthorized, "If the user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "If the user does not have permission.");
    Response(StatusCodes.Status404NotFound, "If the draft is not found.");
  }
}
