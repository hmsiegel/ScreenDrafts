using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AssignSurrogate;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Assign a surrogate prediction set";
    Description = "Links a surrogate set to a primary contestant's set. Used when Ryan cannot co-host and a guest predicts on his behalf.";
    Response(StatusCodes.Status204NoContent, "Surrogate assigned.");
    Response(StatusCodes.Status400BadRequest, "Validation failed or primary/surrogate sets are identical.");
    Response(StatusCodes.Status404NotFound, "Primary or surrogate prediction set not found.");
    Response(StatusCodes.Status403Forbidden, "Insufficient permissions.");
  }
}
