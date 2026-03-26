namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMedia;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddMediaRequest, string>
{
  public override void Configure()
  {
    Post(MoviesRoutes.Base);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
      .WithName(MoviesOpenApi.Names.Media_Add)
      .Produces<Guid>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(MoviesAuth.Permissions.MediaCreate);
  }

  public override async Task HandleAsync(AddMediaRequest req, CancellationToken ct)
  {
    var command = new AddMediaCommand
    {
      PublicId = req.PublicId,
      ImdbId = req.ImdbId,
      TmdbId = req.TmdbId,
      IgdbId = req.IgdbId,
      Title = req.Title,
      Year = req.Year,
      Plot = req.Plot,
      Image = req.Image,
      ReleaseDate = req.ReleaseDate,
      YouTubeTrailerUrl = req.YoutubeTrailerUrl,
      MediaType = req.MediaType,
      TvSeriesTmdbId = req.TvSeriesTmdbId,
      SeasonNumber = req.SeasonNumber,
      EpisodeNumber = req.EpisodeNumber,
      Genres = req.Genres,
      Actors = req.Actors,
      Directors = req.Directors,
      Producers = req.Producers,
      ProductionCompanies = req.ProductionCompanies
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => MovieLocations.ById(created.PublicId),
      ct);
  }
}
