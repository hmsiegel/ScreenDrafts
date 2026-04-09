using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ClosePredictionSeason;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Close a prediction season";
    Description = "Marks a season as closed. After closing, add a Handicap carryover on the next season for Ryan based on the loss margin.";
    Response(StatusCodes.Status204NoContent, "Season closed.");
    Response(StatusCodes.Status404NotFound, "Season not found.");
    Response(StatusCodes.Status409Conflict, "Season is already closed.");
    Response(StatusCodes.Status403Forbidden, "Insufficient permissions.");
  }
}
