namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchForMovie;

// ── Endpoint ──────────────────────────────────────────────────────────────────

internal sealed class Endpoint : ScreenDraftsEndpoint<SearchForMovieRequest, SearchForMovieResponse>
{
  public override void Configure()
  {
    Get(MovieRoutes.Search);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.Movies)
        .WithName(IntegrationsOpenApi.Names.Movies_Search)
        .Produces<SearchForMovieResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    });
    // Any authenticated user may search
  }

  public override async Task HandleAsync(SearchForMovieRequest req, CancellationToken ct)
  {
    var command = new SearchForMovieCommand { Query = req.Query, Page = req.Page };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}
