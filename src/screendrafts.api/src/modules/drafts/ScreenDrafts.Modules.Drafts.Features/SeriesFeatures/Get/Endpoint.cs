namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetSeriesRequest, SeriesResponse>
{
  public override void Configure()
  {
    Get(SeriesRoutes.ById);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Series_GetSeriesById)
      .WithTags(DraftsOpenApi.Tags.Series)
      .Produces<SeriesResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(DraftsAuth.Permissions.SeriesRead);
  }

  public override async Task HandleAsync(GetSeriesRequest req, CancellationToken ct)
  {
    var GetSeriesFeatureQuery = new GetSeriesQuery(req.PublicId);

    var result = await Sender.Send(GetSeriesFeatureQuery, ct);

    await this.SendOkAsync(result, ct);
  }
}


