using FastEndpoints;

using ScreenDrafts.Modules.Drafts.Features.Predictions.Common;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ListPredictionSeasons;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "List prediction seasons.";
    Description = "Returns all Commissioner Predictions seasons with standings, ordered by season number descending.";
    Response<IReadOnlyList<PredictionSeasonSummaryResponse>>(StatusCodes.Status200OK);
  }
}
