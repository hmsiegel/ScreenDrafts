namespace ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMovie;

internal sealed class Endpoint(ISender sender) : ScreenDraftsEndpoint<GetOnlineMovieRequest, GetOnlineMovieResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get(MovieRoutes.Search);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.Movies)
      .WithName(IntegrationsOpenApi.Names.Movies_Search)
      .Produces<GetOnlineMovieResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest);
    });
    Policies(IntegrationsOpenApi.Permissions.MoviesSearch);
  }

  public override async Task HandleAsync(GetOnlineMovieRequest req, CancellationToken ct)
  {
    var query = new GetOnlineMovieCommand(req.ImdbId);

    var result = await _sender.Send(query, ct);

    await this.SendOkAsync<GetOnlineMovieResponse>(result.Value, ct);
  }
}
