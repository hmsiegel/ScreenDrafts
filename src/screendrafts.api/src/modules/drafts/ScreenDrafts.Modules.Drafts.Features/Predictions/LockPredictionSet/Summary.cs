using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.LockPredictionSet;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Lock a prediction set";
    Description = "Locks a prediction set and captures a rules snapshot. Entries cannot be changed after locking.";
    Response(StatusCodes.Status204NoContent, "Set locked.");
    Response(StatusCodes.Status404NotFound, "Draft part or prediction set not found.");
    Response(StatusCodes.Status409Conflict, "Set is already locked.");
    Response(StatusCodes.Status403Forbidden, "Insufficient permissions.");
  }
}
