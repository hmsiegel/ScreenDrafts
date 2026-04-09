namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictions;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetDraftPartPredictionsRequest, IReadOnlyList<DraftPartPredictionResponse>>
{
  public override void Configure()
  {
    Get(PredictionRoutes.GetSets);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_GetSets)
        .Produces<IReadOnlyList<DraftPartPredictionResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PredictionRead);
  }

  public override async Task HandleAsync(
    GetDraftPartPredictionsRequest req,
    CancellationToken ct)
  {
    var query = new GetDraftPartPredictionsQuery 
    { 
      DraftPartPublicId = req.DraftPartPublicId 
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
