namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictors;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<GetDraftPartPredictorsResponse>
{
  public override void Configure()
  {
    Get(PredictionRoutes.SetPredictors);
    AllowAnonymous();
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_GetPredictors)
        .Produces<GetDraftPartPredictorsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    });
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var draftPartId = Route<string>("draftPartId")!;
    var result = await Sender.Send(
      new GetDraftPartPredictorsQuery { DraftPartPublicId = draftPartId },
      ct
    );
    await this.SendOkAsync(result, ct);
  }
}
