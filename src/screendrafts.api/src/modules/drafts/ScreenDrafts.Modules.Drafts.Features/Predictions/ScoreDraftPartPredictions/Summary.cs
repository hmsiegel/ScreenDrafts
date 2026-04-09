using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ScoreDraftPartPredictions;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Score predictions for a draft part";
    Description = "Scores all prediction sets against the finalized pick list. Auto-locks unlocked sets. Applies surrogate merge policies. Returns 409 if already scored.";
    Response(StatusCodes.Status204NoContent, "Scoring completed.");
    Response(StatusCodes.Status400BadRequest, "Validation failed.");
    Response(StatusCodes.Status404NotFound, "Draft part or rules not found.");
    Response(StatusCodes.Status409Conflict, "Already scored.");
    Response(StatusCodes.Status403Forbidden, "Insufficient permissions.");
  }
}
