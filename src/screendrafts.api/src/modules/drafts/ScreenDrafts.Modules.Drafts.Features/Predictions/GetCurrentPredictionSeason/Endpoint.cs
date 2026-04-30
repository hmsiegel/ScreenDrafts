namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetCurrentPredictionSeason;

internal sealed class Endpoint
  : ScreenDraftsEndpointWithoutRequest<PredictionSeasonSummaryResponse>
{
  public override void Configure()
  {
    Get(PredictionRoutes.GetCurrentSeason);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_GetCurrentSeason)
        .Produces<PredictionSeasonSummaryResponse>(StatusCodes.Status200OK);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new GetCurrentPredictionSeasonQuery();
    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
