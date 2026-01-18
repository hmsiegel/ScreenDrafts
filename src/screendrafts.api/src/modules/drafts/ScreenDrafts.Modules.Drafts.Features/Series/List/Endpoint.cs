namespace ScreenDrafts.Modules.Drafts.Features.Series.List;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<SeriesCollectionResponse>
{
  public override void Configure()
  {
    Get(SeriesRoutes.Series);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Series_ListSeries)
      .WithTags(DraftsOpenApi.Tags.Series)
      .Produces<SeriesCollectionResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(Features.Permissions.SeriesList);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new Query();

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
