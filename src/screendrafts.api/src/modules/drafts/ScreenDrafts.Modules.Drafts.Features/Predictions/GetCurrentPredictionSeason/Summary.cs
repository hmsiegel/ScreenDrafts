using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetCurrentPredictionSeason;

// ============================================================
// Summary
// ============================================================

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get the current prediction season";
    Description = "Returns the active (non-closed) prediction season with standings. Intended for the home page standings widget.";
    Response<PredictionSeasonSummaryResponse>(StatusCodes.Status200OK);
  }
}
