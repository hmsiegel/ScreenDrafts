namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByIgdbIds;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetMediaByIgdbIdsRequest, GetMediaByIgdbIdsResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.MediaByIgdbIds);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MoviesOpenApi.Names.Media_GetMediaByIgdbIds)
        .Produces<GetMediaByIgdbIdsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    });
  }

  public override async Task HandleAsync(GetMediaByIgdbIdsRequest req, CancellationToken ct)
  {
    var query = new GetMediaByIgdbIdsQuery { IgdbIds = req.IgdbIds };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
