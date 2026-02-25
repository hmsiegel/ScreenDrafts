using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ApplyVetoOverride;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Applies a veto override to a vetoed pick.";
    Description = "Overrides a veto, locking in the pick in the position it was played.";
    Response(StatusCodes.Status204NoContent, "Veto override applied successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request data.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access.");
    Response(StatusCodes.Status404NotFound, "Draft part not found.");
  }
}
