using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ListPredictionSeasons;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get a list of all prediction seasons and the drafts each one encompasses.";
    Description =
      "Returns every prediction season, newest first, each with the list of draft "
      + "parts that had a prediction set submitted against it during that season. "
      + "Patreon-exclusive draft parts are omitted for callers without Patreon read access.";
    Response<ListPredictionSeasonsResponse>(
      StatusCodes.Status200OK,
      "A list of prediction seasons with their associated drafts."
    );
  }
}
