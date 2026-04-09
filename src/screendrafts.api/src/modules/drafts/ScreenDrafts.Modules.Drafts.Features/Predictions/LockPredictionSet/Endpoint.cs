namespace ScreenDrafts.Modules.Drafts.Features.Predictions.LockPredictionSet;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<LockPredictionSetRequest>
{
  public override void Configure()
  {
    Put(PredictionRoutes.LockSet);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_LockSet)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.PredictionManage);
  }

  public override async Task HandleAsync(LockPredictionSetRequest req, CancellationToken ct)
  {
    var command = new LockPredictionSetCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      SetPublicId = req.SetPublicId
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
