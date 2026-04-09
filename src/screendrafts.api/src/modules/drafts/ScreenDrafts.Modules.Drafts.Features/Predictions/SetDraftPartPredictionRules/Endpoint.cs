namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictionRules;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<SetDraftPartPredictionRulesRequest>
{
  public override void Configure()
  {
    Post(PredictionRoutes.SetRules);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_SetRules)
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
    SetDraftPartPredictionRulesRequest req,
    CancellationToken ct)
  {
    var command = new SetDraftPartPredictionRulesCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      PredictionMode = req.PredictionMode,
      RequiredCount = req.RequiredCount,
      TopN = req.TopN,
      DeadlineUtc = req.DeadlineUtc
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
