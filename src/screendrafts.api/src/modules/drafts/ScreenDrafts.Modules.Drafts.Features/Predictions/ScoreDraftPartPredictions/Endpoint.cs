namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ScoreDraftPartPredictions;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<ScoreDraftPartPredictionsRequest>
{
  public override void Configure()
  {
    Post(PredictionRoutes.ScoreSets);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_ScoreSets)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.PredictionManage);
  }

  public override async Task HandleAsync(
    ScoreDraftPartPredictionsRequest req,
    CancellationToken ct)
  {
    var command = new ScoreDraftPartPredictionsCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      FinalMediaPublicIds = req.FinalMediaPublicIds
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
