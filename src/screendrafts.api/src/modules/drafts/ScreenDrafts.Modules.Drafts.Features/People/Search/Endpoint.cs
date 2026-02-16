namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed class Endpoint : ScreenDraftsEndpoint<SearchPeopleRequest, PeopleSearchResponse>
{
  public override void Configure()
  {
    Get(PeopleRoutes.Search);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.People_SearchPeople)
      .WithTags(DraftsOpenApi.Tags.People)
      .Produces<PeopleSearchResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(DraftsAuth.Permissions.PersonSearch);
  }

  public override async Task HandleAsync(SearchPeopleRequest req, CancellationToken ct)
  {
    var SearchPeopleQuery = new SearchPeopleQuery(req.Search, req.Limit);

    var result = await Sender.Send(SearchPeopleQuery, ct);

    await this.SendOkAsync(result, ct);
  }
}


