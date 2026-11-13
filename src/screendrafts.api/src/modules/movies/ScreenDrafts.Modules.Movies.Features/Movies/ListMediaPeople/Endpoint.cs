namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMediaPeople;

internal sealed class Endpoint : ScreenDraftsEndpoint<ListMediaPeopleRequest, ListMediaPeopleResponse>
{
  public override void Configure()
  {
    Get(MediaPeopleListRoutes.Browse);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MediaPeopleListRoutes.BrowseOperationName)
        .Produces<ListMediaPeopleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(ListMediaPeopleRequest req, CancellationToken ct)
  {
    var query = new ListMediaPeopleQuery
    {
      Page = req.Page,
      PageSize = req.PageSize,
      Search = req.Search,
      Role = req.Role,
      Sort = req.Sort,
    };

    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
