namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AssignSurrogate;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<AssignSurrogateRequest>
{
  public override void Configure()
  {
    Post(PredictionRoutes.AssignSurrogate);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_AssignSurrogate)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PredictionManage);
  }

  public override async Task HandleAsync(AssignSurrogateRequest req, CancellationToken ct)
  {
    var command = new AssignSurrogateCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      PrimarySetPublicId = req.PrimarySetPublicId,
      SurrogateSetPublicId = req.SurrogateSetPublicId,
      MergePolicy = req.MergePolicy
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
