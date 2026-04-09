using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictionRules;

internal sealed class Summary : Summary<Endpoint>

{
  public Summary()
  {
    Summary = "Set prediction rules for a draft part";
    Description = "Defines how Commissioner Predictions are scored for a specific draft part. Can only be set once — update not supported by design.";
    Response(StatusCodes.Status204NoContent, "The rules were successfully set.");
    Response(StatusCodes.Status400BadRequest, "Validation failed or invalid mode/TopN combination.");
    Response(StatusCodes.Status401Unauthorized, "Authentication required.");
    Response(StatusCodes.Status403Forbidden, "Insufficient permissions.");
    Response(StatusCodes.Status404NotFound, "Draft part not found.");
    Response(StatusCodes.Status409Conflict, "Rules already exist for this draft part.");
  }
}
