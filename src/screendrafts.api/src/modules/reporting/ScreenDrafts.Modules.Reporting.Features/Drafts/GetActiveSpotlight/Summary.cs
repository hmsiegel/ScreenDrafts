using FastEndpoints;

namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetActiveSpotlight;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get the active spotlight draft.";
    Description = "Gets the active spotlight draft.";
    Response(StatusCodes.Status200OK, "The active spotlight draft was retrieved successfully.");
    Response(StatusCodes.Status404NotFound, "No active spotlight draft was found.");
  }
}
