namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.List;

internal sealed class Endpoint : ScreenDraftsEndpoint<ListSeriesRequest, SeriesCollectionResponse>
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
    Policies(DraftsAuth.Permissions.SeriesList);
  }

  public override async Task HandleAsync(ListSeriesRequest req, CancellationToken ct)
  {
    var isAdmin = User.HasPermission(DraftsAuth.Permissions.AdminViewDeleted);

    if (!isAdmin && req.IncludeDeleted)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, ct);
      return;
    }

    var query = new ListSeriesQuery { IncludeDeleted = req.IncludeDeleted };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
