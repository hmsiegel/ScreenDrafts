namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchGamesMedia;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<SearchGamesMediaRequest, SearchGamesMediaResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.GamesMediaSearch);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MoviesOpenApi.Names.Media_SearchGames)
        .Produces<SearchGamesMediaResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(SearchGamesMediaRequest req, CancellationToken ct)
  {
    var query = new SearchGamesMediaQuery { Query = req.Query, Page = req.Page };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
