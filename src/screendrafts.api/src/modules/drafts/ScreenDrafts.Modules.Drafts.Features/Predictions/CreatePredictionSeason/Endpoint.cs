namespace ScreenDrafts.Modules.Drafts.Features.Predictions.CreatePredictionSeason;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreatePredictionSeasonRequest, CreatedResponse>
{
  public override void Configure()
  {
    Post(PredictionRoutes.CreateSeason);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
      .WithName(DraftsOpenApi.Names.Predictions_CreateSeason)
      .Produces<CreatedResponse>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.PredictionManage);
  }

  public override async Task HandleAsync(CreatePredictionSeasonRequest req, CancellationToken ct)
  {
    var command = new CreatePredictionSeasonCommand
    {
      Number = req.Number,
      StartsOn = req.StartsOn
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => PredictionLocations.SeasonById(created.PublicId),
      ct);
  }
}
