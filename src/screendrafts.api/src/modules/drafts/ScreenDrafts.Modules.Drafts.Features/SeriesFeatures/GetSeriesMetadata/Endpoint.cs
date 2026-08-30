namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.GetSeriesMetadata;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<GetSeriesMetadataResponse>
{
  public override void Configure()
  {
    Get(SeriesRoutes.Metadata);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Series_GetSeriesMetadata)
        .WithTags(DraftsOpenApi.Tags.Series)
        .Produces<GetSeriesMetadataResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.SeriesRead);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var MetadataSeriesFeatureQuery = new GetSeriesMetadataQuery();
    var result = await Sender.Send(MetadataSeriesFeatureQuery, ct);
    await this.SendOkAsync(result, ct);
  }
}
