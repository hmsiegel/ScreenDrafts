namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ClosePredictionSeason;

internal sealed class Endpoint : ScreenDraftsEndpoint<ClosePredictionSeasonRequest>

{
  public override void Configure()
  {
    Post(PredictionRoutes.CloseSeason);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_CloseSeason)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.PredictionManage);
  }

  public override async Task HandleAsync(ClosePredictionSeasonRequest req, CancellationToken ct)
  {
    var command = new ClosePredictionSeasonCommand
    {
      SeasonPublicId = req.SeasonPublicId,
      EndsOn = req.EndsOn
    };
    
    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
