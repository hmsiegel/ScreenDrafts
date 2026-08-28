namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ListPredictionSeasons;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<ListPredictionSeasonsResponse>
{
  public override void Configure()
  {
    Get(PredictionRoutes.Seasons);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_ListSeasons)
        .Produces<ListPredictionSeasonsResponse>(StatusCodes.Status200OK);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var includePatreon =
      User.Identity?.IsAuthenticated == true
      && User.HasPermission(DraftsAuth.Permissions.DraftReadPatreon);

    var query = new ListPredictionSeasonsQuery { IncludePatreon = includePatreon };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
