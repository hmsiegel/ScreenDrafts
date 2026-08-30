namespace ScreenDrafts.Modules.Integrations.Features.Movies.BrowseSeasonEpisodes;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<BrowseSeasonEpisodesRequest, BrowseSeasonEpisodesResponse>
{
  public override void Configure()
  {
    Get(MovieRoutes.BrowseSeasonEpisodes);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.Movies)
        .WithName(IntegrationsOpenApi.Names.Movies_BrowseSeasonEpisodes)
        .Produces<BrowseSeasonEpisodesResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    });
    // Any authenticated user may browse — same permission level as movie search.
  }

  public override async Task HandleAsync(BrowseSeasonEpisodesRequest req, CancellationToken ct)
  {
    var command = new BrowseSeasonEpisodesCommand
    {
      SeriesTmdbId = req.SeriesTmdbId,
      SeasonNumber = req.SeasonNumber,
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}
