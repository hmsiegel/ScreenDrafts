namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByImdbIds;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetMediaByImdbIdsRequest, GetMediaByImdbIdsResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.MediaByImdbIds);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MoviesOpenApi.Names.Media_GetMediaByImdbIds)
        .Produces<GetMediaByImdbIdsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    });
  }

  public override async Task HandleAsync(GetMediaByImdbIdsRequest req, CancellationToken ct)
  {
    var query = new GetMediaByImdbIdsQuery { ImdbIds = req.ImdbIds };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
