namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByTmdbIds;

// ── Endpoint ──────────────────────────────────────────────────────────────────

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetMediaByTmdbIdsRequest, GetMediaByTmdbIdsResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.MediaByTmdbIds);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MoviesOpenApi.Names.Media_GetMediaByTmdbIds)
        .Produces<GetMediaByTmdbIdsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    });
  }

  public override async Task HandleAsync(GetMediaByTmdbIdsRequest req, CancellationToken ct)
  {
    var query = new GetMediaByTmdbIdsQuery { TmdbIds = req.TmdbIds };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
