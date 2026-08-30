namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchForTvShow;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<SearchForTvShowRequest, SearchForTvShowResponse>
{
  public override void Configure()
  {
    Get(MovieRoutes.SearchTv);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.Movies)
        .WithName(IntegrationsOpenApi.Names.Movies_SearchTv)
        .Produces<SearchForTvShowResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    });
  }

  public override async Task HandleAsync(SearchForTvShowRequest req, CancellationToken ct)
  {
    var command = new SearchForTvShowCommand { Query = req.Query, Page = req.Page };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}
