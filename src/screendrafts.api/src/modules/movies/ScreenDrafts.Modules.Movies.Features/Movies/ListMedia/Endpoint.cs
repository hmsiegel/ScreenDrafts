namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMedia;

internal sealed class Endpoint : ScreenDraftsEndpoint<ListMediaRequest, ListMediaResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.Base);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MoviesOpenApi.Names.Media_ListMedia)
        .Produces<ListMediaResponse>(StatusCodes.Status200OK);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(ListMediaRequest req, CancellationToken ct)
  {
    var query = new ListMediaQuery
    {
      Page = req.Page,
      PageSize = req.PageSize,
      Search = req.Search,
      MediaType = req.MediaType,
      Year = req.Year,
      Sort = req.Sort,
    };

    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
