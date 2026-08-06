namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByExternalIds;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetMediaByExternalIdsRequest, GetMediaByExternalIdsResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.MediaByExternalIds);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MoviesOpenApi.Names.Media_GetMediaByExternalIds)
        .Produces<GetMediaByExternalIdsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    });
  }

  public override async Task HandleAsync(GetMediaByExternalIdsRequest req, CancellationToken ct)
  {
    var query = new GetMediaByExternalIdsQuery { ExternalIds = req.ExternalIds };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
