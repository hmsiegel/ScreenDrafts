using FastEndpoints;

namespace ScreenDrafts.Modules.Reporting.Features.Drafts.DeleteSpotlight;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Delete spotlight for a draft.";
    Description = "Deletes spotlight for a draft, making it no longer visible to users.";
    Response(StatusCodes.Status204NoContent, "Spotlight deleted successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
  }
}
