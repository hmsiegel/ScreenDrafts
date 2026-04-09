using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SubmitPredictionSet;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Submit a prediction set";
    Description = "Submits a contestant's film predictions for a draft part. Entry count must match the rules RequiredCount. Use SourceKind=1 for CSV uploads.";
    Response(StatusCodes.Status204NoContent, "Predictions submitted.");
    Response(StatusCodes.Status400BadRequest, "Validation failed or entry count mismatch.");
    Response(StatusCodes.Status404NotFound, "Draft part, season, or contestant not found.");
    Response(StatusCodes.Status409Conflict, "A set already exists for this contestant and draft part, or the deadline has passed.");
    Response(StatusCodes.Status403Forbidden, "Insufficient permissions.");
  }
}
