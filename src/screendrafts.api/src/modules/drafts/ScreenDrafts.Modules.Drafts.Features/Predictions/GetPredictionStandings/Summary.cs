using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetPredictionStandings;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get prediction standings for a season";
    Description = "Returns all contestant standings for a prediction season, including carryover points. Sorted by total points descending.";
    Response<PredictionStandingsResponse>(StatusCodes.Status200OK);
    Response(StatusCodes.Status404NotFound, "Season not found.");
    Response(StatusCodes.Status403Forbidden, "Insufficient permissions.");
  }
}
