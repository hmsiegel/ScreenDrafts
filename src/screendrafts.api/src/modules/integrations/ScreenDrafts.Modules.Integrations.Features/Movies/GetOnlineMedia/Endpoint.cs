namespace ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMedia;

internal sealed class Endpoint(ISender sender) : ScreenDraftsEndpoint<GetOnlineMediaRequest, GetOnlineMediaResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get(MovieRoutes.Search);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.Movies)
      .WithName(IntegrationsOpenApi.Names.Movies_Search)
      .Produces<GetOnlineMediaResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest);
    });
    Policies(IntegrationsOpenApi.Permissions.MoviesSearch);
  }

  public override async Task HandleAsync(GetOnlineMediaRequest req, CancellationToken ct)
  {
    var command = new GetOnlineMediaCommand
    {
      TmdbId = req.TmdbId,
      IgdbId = req.IgdbId,
      ImdbId = req.ImdbId,
      MediaType = req.MediaType,
      EpisodeNumber = req.EpisodeNumber,
      SeasonNumber = req.SeasonNumber,
      TvSeriesTmdbId = req.TvSeriesTmdbId
    };

    var result = await _sender.Send(command, ct);

    await this.SendOkAsync<GetOnlineMediaResponse>(result.Value, ct);
  }
}
