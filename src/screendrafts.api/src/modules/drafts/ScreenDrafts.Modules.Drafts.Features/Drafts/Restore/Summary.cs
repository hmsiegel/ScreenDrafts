using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Restore;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Restores a draft.";
    Description = "Restores a previously deleted draft, making it active again.";
    Response(StatusCodes.Status204NoContent, "Draft restored successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
    Response(StatusCodes.Status409Conflict, "Draft cannot be restored due to a conflict.");
  }
}
