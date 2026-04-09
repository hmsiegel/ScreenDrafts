namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AddCarryover;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddCarryoverRequest>
{
  public override void Configure()
  {
    Post(PredictionRoutes.AddCarryover);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_AddCarryover)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PredictionManage);
  }

  public override async Task HandleAsync(AddCarryoverRequest req, CancellationToken ct)
  {
    var command = new AddCarryoverCommand
    {
      SeasonPublicId = req.SeasonPublicId,
      ContestantPublicId = req.ContestantPublicId,
      Points = req.Points,
      Kind = req.Kind,
      Reason = req.Reason
    };

    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}
