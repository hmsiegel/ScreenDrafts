namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMedia;

internal sealed class Endpoint : ScreenDraftsEndpoint<SearchMediaRequest, SearchMediaResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.MediaSearch);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
      .WithName(MoviesOpenApi.Names.Media_Search)
      .Produces<SearchMediaResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest);
    });
    Policies(MoviesAuth.Permissions.MediaSearch);
  }

  public override async Task HandleAsync(SearchMediaRequest req, CancellationToken ct)
  {
    var query = new SearchMediaQuery
    {
      DraftPartId = req.DraftPartId,
      Query = req.Query,
      Year = req.Year,
      Page = req.Page,
      PageSize = req.PageSize
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
