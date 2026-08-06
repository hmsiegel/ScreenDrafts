namespace ScreenDrafts.Modules.Movies.Features.Movies.GetPersonFilmographyMedia;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetPersonFilmographyMediaRequest, GetPersonFilmographyMediaResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.PersonFilmographyMedia);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MoviesOpenApi.Names.Media_GetPersonFilmography)
        .Produces<GetPersonFilmographyMediaResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetPersonFilmographyMediaRequest req, CancellationToken ct)
  {
    var query = new GetPersonFilmographyMediaQuery { ImdbId = req.ImdbId };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
