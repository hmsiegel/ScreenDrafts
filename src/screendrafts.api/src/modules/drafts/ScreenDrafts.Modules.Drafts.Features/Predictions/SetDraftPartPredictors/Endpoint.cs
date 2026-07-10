namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictors;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetDraftPartPredictorsRequest>
{
  public override void Configure()
  {
    Put(PredictionRoutes.SetPredictors);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_SetPredictors)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.PredictionManage);
  }

  public override async Task HandleAsync(SetDraftPartPredictorsRequest req, CancellationToken ct)
  {
    var command = new SetDraftPartPredictorsCommand
    {
      DraftPartPublicId = req.DraftPartId,
      Predictors =
      [
        .. req.Predictors.Select(p => new PredictorEntryDto
        {
          ContestantPublicId = p.ContestantPublicId,
          AllowedSubmitterPersonPublicId = p.AllowedSubmitterPersonPublicId,
        }),
      ],
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
