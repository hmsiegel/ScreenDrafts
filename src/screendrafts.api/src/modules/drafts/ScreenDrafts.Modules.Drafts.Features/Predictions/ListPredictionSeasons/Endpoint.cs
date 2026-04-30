namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ListPredictionSeasons;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<ListPredictionSeasonsResult>
{
  public override void Configure()
  {
    Get(PredictionRoutes.Seasons);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
      .WithName(DraftsOpenApi.Names.Predictions_ListSeasons)
      .Produces<ListPredictionSeasonsResult>(StatusCodes.Status200OK);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new ListPredictionSeasonsQuery();

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
