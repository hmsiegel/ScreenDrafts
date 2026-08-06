namespace ScreenDrafts.Modules.Integrations.Features.YouTube.SearchYouTube;

internal sealed class Endpoint : ScreenDraftsEndpoint<SearchYouTubeRequest, SearchYouTubeResponse>
{
  public override void Configure()
  {
    Get(YouTubeRoutes.YouTubeSearch);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.YouTube)
        .WithName(IntegrationsOpenApi.Names.YouTube_Search)
        .Produces<SearchYouTubeResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    });
  }

  public override async Task HandleAsync(SearchYouTubeRequest req, CancellationToken ct)
  {
    var command = new SearchYouTubeCommand { Query = req.Query, Page = req.Page };
    var result = await Sender.Send(command, ct);
    await this.SendOkAsync(result, ct);
  }
}
