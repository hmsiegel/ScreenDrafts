namespace ScreenDrafts.Modules.Integrations.Features.Movies.FetchMedia;

internal sealed class Endpoint : ScreenDraftsEndpoint<FetchMediaRequest>
{
  public override void Configure()
  {
    Post(MovieRoutes.Import);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.Movies)
        .WithName(IntegrationsOpenApi.Names.Movies_Import)
        .Produces(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(IntegrationsOpenApi.Permissions.MoviesImport);
  }

  public override async Task HandleAsync(FetchMediaRequest req, CancellationToken ct)
  {
    var command = new FetchMediaCommand
    {
      MediaType = MediaType.FromValue(req.MediaType),
      ImdbId = req.ImdbId,
      TmdbId = req.TmdbId,
      IgdbId = req.IgdbId,
      TvSeriesTmdbId = req.TvSeriesTmdbId,
      SeasonNumber = req.SeasonNumber,
      EpisodeNumber = req.EpisodeNumber,
    };

    var result = await Sender.Send(command, ct);

    await this.SendAcceptedAsync(result, ct);
  }
}
