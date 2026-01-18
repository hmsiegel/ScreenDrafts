namespace ScreenDrafts.Modules.Drafts.Features.Series.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, SeriesResponse>
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
    Permissions(Features.Permissions.SeriesRead);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var query = new Query(req.PublicId);

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
