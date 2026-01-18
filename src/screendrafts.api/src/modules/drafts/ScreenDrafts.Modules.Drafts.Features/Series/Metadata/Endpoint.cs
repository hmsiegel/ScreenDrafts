namespace ScreenDrafts.Modules.Drafts.Features.Series.Metadata;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<Response>
{
  public override void Configure()
  {
    Get(SeriesRoutes.Metadata);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Series_GetSeriesMetadata)
      .WithTags(DraftsOpenApi.Tags.Series)
      .Produces<Response>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(Features.Permissions.SeriesRead);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new Query();
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
