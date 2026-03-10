namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMovies;

internal sealed class Endpoint : ScreenDraftsEndpoint<SearchMoviesRequest, SearchMoviesResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.MovieSearch);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Movies)
      .WithName(MoviesOpenApi.Names.Movies_SearchMovies)
      .Produces<SearchMoviesResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest);
    });
    Policies(MoviesAuth.Permissions.MoviesSearch);
  }

  public override async Task HandleAsync(SearchMoviesRequest req, CancellationToken ct)
  {
    var query = new SearchMoviesQuery
    {
      DraftPartId = req.DraftPartId,
      Query = req.Query,
      Year = req.Year,
      Page = req.Page,
      PageSize = req.PageSize
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
