using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Set the status of a draft part (start or complete).";
    Description = "Sets the status of a specific part of a draft identified by its public ID and part index.";
    Response<Response>(StatusCodes.Status200OK, "The draft part status has been successfully updated.");
    Response(StatusCodes.Status400BadRequest, "The SetDraftPartStatusRequest was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to perform this action.");
    Response(StatusCodes.Status404NotFound, "The specified draft or draft part was not found.");
  }
}

