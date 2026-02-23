using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategory;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Set the category of a draft.";
    Description = "Set the category of a draft.";
    Response(StatusCodes.Status204NoContent, "The category was set successfully.");
    Response(StatusCodes.Status400BadRequest, "The request is invalid.");
    Response(StatusCodes.Status404NotFound, "The draft or category was not found.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to set the category of the draft.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
  }
}
