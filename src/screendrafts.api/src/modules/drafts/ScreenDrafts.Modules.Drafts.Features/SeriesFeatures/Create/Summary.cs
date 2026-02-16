using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Create a new series";
    Description = "Creates a new series with the specified parameters.";
    Response<CreatedIdResponse>(StatusCodes.Status201Created, "The Id of the created series.");
    Response(StatusCodes.Status400BadRequest, "Invalid CreateSeriesFeatureRequest.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to create a series.");
  }
}



