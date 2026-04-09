using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.CreatePredictionSeason;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Creates a new prediction season.";
    Description = "Creates a new prediction season with the specified number, start date, public ID, and a target of 100 points.";
    Response<CreatedResponse>(StatusCodes.Status201Created, "The prediction season was created successfully.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to create a prediction season.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authorized to create a prediction season.");
  }
}
