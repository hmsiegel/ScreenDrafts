namespace ScreenDrafts.Modules.Drafts.Features.Predictions.CreatePredictionContestant;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreatePredictionContestantRequest, CreatedResponse>
{
  public override void Configure()
  {
    Post(PredictionRoutes.CreateContestant);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_CreateContestant)
        .Produces<CreatedResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.PredictionManage);
  }

  public override async Task HandleAsync(
    CreatePredictionContestantRequest req,
    CancellationToken ct)
  {
    var command = new CreatePredictionContestantCommand
    {
      PersonPublicId = req.PersonPublicId
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result: result.Map(value => new CreatedResponse(value)),
      locationFactory: id => PredictionLocations.ContestantById(id.PublicId),
      cancellationToken: ct);
  }
}
