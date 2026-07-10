namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictionRules;

internal sealed class Endpoint
  : ScreenDraftsEndpointWithoutRequest<GetDraftPartPredictionRulesResponse>
{
  public override void Configure()
  {
    Get(PredictionRoutes.SetRules);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_GetRules)
        .Produces<GetDraftPartPredictionRulesResponse?>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PredictionManage);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var draftPartId = Route<string>("draftPartId")!;
    var result = await Sender.Send(
      new GetDraftPartPredictionRulesQuery { DraftPartPublicId = draftPartId },
      ct
    );
    await this.SendOkAsync(result, ct);
  }
}
