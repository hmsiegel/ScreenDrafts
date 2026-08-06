namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchYouTubeMedia;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<SearchYouTubeMediaRequest, SearchYouTubeMediaResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.YouTubeMediaSearch);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MoviesOpenApi.Names.Media_SearchYouTube)
        .Produces<SearchYouTubeMediaResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(SearchYouTubeMediaRequest req, CancellationToken ct)
  {
    var query = new SearchYouTubeMediaQuery { Query = req.Query, Page = req.Page };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
