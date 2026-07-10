namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SearchPredictionContestants;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<SearchPredictionContestantsRequest, SearchPredictionContestantsResponse>
{
  public override void Configure()
  {
    Get(PredictionRoutes.SearchContestants);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_SearchContestants)
        .Produces<SearchPredictionContestantsResponse>(StatusCodes.Status200OK);
    });
    Policies(DraftsAuth.Permissions.PredictionManage);
  }

  public override async Task HandleAsync(
    SearchPredictionContestantsRequest req,
    CancellationToken ct
  )
  {
    var result = await Sender.Send(
      new SearchPredictionContestantsQuery { Name = req.Name, PageSize = req.PageSize },
      ct
    );

    await this.SendOkAsync(result, ct);
  }
}

internal sealed record SearchPredictionContestantsRequest
{
  [FromQuery(Name = "name")]
  public string? Name { get; init; }

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; } = 20;
}
