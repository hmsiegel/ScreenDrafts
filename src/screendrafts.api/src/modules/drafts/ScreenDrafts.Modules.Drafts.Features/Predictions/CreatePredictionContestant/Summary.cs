using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.CreatePredictionContestant;

internal sealed class Summary : Summary<Endpoint>

{
  public Summary()
  {
    Summary = "Create a prediction contestant";
    Description = "Promotes a Person into a Commissioner Predictions contestant. One contestant per person.";
    Response<CreatedResponse>(StatusCodes.Status201Created, "The public ID of the new contestant.");
    Response(StatusCodes.Status400BadRequest, "Validation failed.");
    Response(StatusCodes.Status404NotFound, "Person not found.");
    Response(StatusCodes.Status409Conflict, "Contestant already exists for this person.");
    Response(StatusCodes.Status403Forbidden, "Insufficient permissions.");
  }
}
