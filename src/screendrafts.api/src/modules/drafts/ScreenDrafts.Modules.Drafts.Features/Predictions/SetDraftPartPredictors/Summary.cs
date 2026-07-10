using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictors;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Set Draft Part Predictors";
    Description = "Sets the predictors for this draft part.";
    Response(StatusCodes.Status204NoContent, "Draft Part Predictors set successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status404NotFound, "Draft Part not found.");
    Response(StatusCodes.Status409Conflict, "Draft Part already has predictors set.");
  }
}
