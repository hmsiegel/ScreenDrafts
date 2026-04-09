using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictions;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get all prediction sets for a draft part";
    Description = "Returns all contestant prediction sets for a draft part, including entries and scored results if available.";
    Response<IReadOnlyList<DraftPartPredictionResponse>>(StatusCodes.Status200OK);
    Response(StatusCodes.Status404NotFound, "Draft part not found.");
    Response(StatusCodes.Status403Forbidden, "Insufficient permissions.");
  }
}
