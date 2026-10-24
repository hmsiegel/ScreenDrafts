namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPublicIdByImdbId;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetMediaPublicIdByImdbIdRequest, GetMediaPublicIdByImdbIdResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.GetMediaPublicIdByImdbId);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MoviesOpenApi.Names.Media_GetPublicIdByImdbId)
        .Produces<GetMediaPublicIdByImdbIdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetMediaPublicIdByImdbIdRequest req, CancellationToken ct)
  {
    var result = await Sender.Send(new GetMediaPublicIdByImdbIdQuery { ImdbId = req.ImdbId }, ct);
    await this.SendOkAsync(result, ct);
  }
}
