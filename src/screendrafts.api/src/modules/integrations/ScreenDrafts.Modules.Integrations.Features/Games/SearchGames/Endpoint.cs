namespace ScreenDrafts.Modules.Integrations.Features.Games.SearchGames;

internal sealed class Endpoint : ScreenDraftsEndpoint<SearchGamesRequest, SearchGamesResponse>
{
  public override void Configure()
  {
    Get(GamesRoutes.GamesSearch);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.Games)
        .WithName(IntegrationsOpenApi.Names.Games_Search)
        .Produces<SearchGamesResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    });
    // Any authenticated user may search — same as SearchForMovie.
  }

  public override async Task HandleAsync(SearchGamesRequest req, CancellationToken ct)
  {
    var command = new SearchGamesCommand { Query = req.Query, Page = req.Page };
    var result = await Sender.Send(command, ct);
    await this.SendOkAsync(result, ct);
  }
}
