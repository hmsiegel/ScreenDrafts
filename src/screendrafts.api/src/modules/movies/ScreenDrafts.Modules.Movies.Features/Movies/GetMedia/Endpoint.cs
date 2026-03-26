namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetMediaRequest, MediaResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.GetMedia);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
      .WithName(MoviesOpenApi.Names.Media_Get)
      .Produces<MediaResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(MoviesAuth.Permissions.MediaRead);
  }

  public override async Task HandleAsync(GetMediaRequest req, CancellationToken ct)
  {
    var query = new GetMediaQuery { PublicId = req.PublicId };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
