namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMovie;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetMovieRequest, MovieResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.GetMovie);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Movies)
      .WithName(MoviesOpenApi.Names.Movies_GetMovie)
      .Produces<MovieResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(MoviesAuth.Permissions.MoviesRead);
  }

  public override async Task HandleAsync(GetMovieRequest req, CancellationToken ct)
  {
    var query = new GetMovieQuery(req.ImdbId);
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}

internal sealed record GetMovieRequest
{
  [FromRoute(Name = "imdbId")]
  public required string ImdbId { get; init; }
}
