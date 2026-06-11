using FastEndpoints;

namespace ScreenDrafts.Modules.Reporting.Features.Drafts.DeactivateSpotlight;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Deactivate spotlight for a draft.";
    Description = "Deactivates spotlight for a draft, making it invisible to users.";
    Response(StatusCodes.Status204NoContent, "Spotlight deactivated successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
  }
}
