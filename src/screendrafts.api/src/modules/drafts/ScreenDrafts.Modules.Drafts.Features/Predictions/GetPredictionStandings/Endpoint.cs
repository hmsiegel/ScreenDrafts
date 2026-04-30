namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetPredictionStandings;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetPredictionStandingsRequest, PredictionStandingsResponse>
{
  public override void Configure()
  {
    Get(PredictionRoutes.GetSeasonStandings);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_GetSeasonStandings)
        .Produces<PredictionStandingsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PredictionRead);
  }

  public override async Task HandleAsync(
    GetPredictionStandingsRequest req,
    CancellationToken ct)
  {
    var query = new GetPredictionStandingsQuery
    {
      SeasonPublicId = req.PublicId
    };
    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
