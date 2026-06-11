using FastEndpoints;

namespace ScreenDrafts.Modules.Reporting.Features.Drafts.ActivateSpotlight;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Activate spotlight for a draft.";
    Description = "Activates spotlight for a draft, making it visible to users.";
    Response(StatusCodes.Status204NoContent, "Spotlight activated successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
  }
}
