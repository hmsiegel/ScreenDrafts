namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Metadata;

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
    Permissions(DraftsAuth.Permissions.SeriesRead);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var MetadataSeriesFeatureQuery = new MetadataSeriesQuery();
    var result = await Sender.Send(MetadataSeriesFeatureQuery, ct);
    await this.SendOkAsync(result, ct);
  }
}

