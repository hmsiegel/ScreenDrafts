using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AddCarryover;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Add a carryover to a prediction season";
    Description = "Records a points carryover for a contestant. Use Kind=0 (Handicap) for Ryan's season-start head-start. Use Kind=1 (Bonus) for commissioner-awarded points.";
    Response(StatusCodes.Status204NoContent, "Carryover recorded.");
    Response(StatusCodes.Status400BadRequest, "Validation failed.");
    Response(StatusCodes.Status404NotFound, "Season or contestant not found.");
    Response(StatusCodes.Status403Forbidden, "Insufficient permissions.");
  }
}
