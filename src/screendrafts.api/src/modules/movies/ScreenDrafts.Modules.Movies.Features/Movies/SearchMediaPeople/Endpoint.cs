namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMediaPeople;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<SearchMediaPeopleRequest, SearchMediaPeopleResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.MediaPeople);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MoviesOpenApi.Names.Media_SearchPeople)
        .Produces<SearchMediaPeopleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(SearchMediaPeopleRequest req, CancellationToken ct)
  {
    var query = new SearchMediaPeopleQuery
    {
      Query = req.Query,
      Role = req.Role,
      Limit = req.Limit,
    };

    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
