namespace ScreenDrafts.Modules.Integrations.Features.Movies.BrowseTvSeasons;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<BrowseTvSeasonsRequest, BrowseTvSeasonsResponse>
{
  public override void Configure()
  {
    Get(MovieRoutes.BrowseTvSeasons);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.Movies)
        .WithName(IntegrationsOpenApi.Names.Movies_BrowseTvSeasons)
        .Produces<BrowseTvSeasonsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    });
    // Any authenticated user may browse — same permission level as BrowseSeasonEpisodes.
  }

  public override async Task HandleAsync(BrowseTvSeasonsRequest req, CancellationToken ct)
  {
    var command = new BrowseTvSeasonsCommand { SeriesTmdbId = req.SeriesTmdbId };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}
