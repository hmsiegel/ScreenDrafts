namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed class Endpoint : ScreenDraftsEndpoint<SearchPeopleRequest, PagedResult<SearchPeopleResponse>>
{
  public override void Configure()
  {
    Get(PeopleRoutes.Search);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.People_SearchPeople)
      .WithTags(DraftsOpenApi.Tags.People)
      .Produces<PagedResult<SearchPeopleResponse>>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(DraftsAuth.Permissions.PersonSearch);
  }

  public override async Task HandleAsync(SearchPeopleRequest req, CancellationToken ct)
  {
    var query = new SearchPeopleQuery
    {
      Name = req.Name,
      Role = req.Role,
      Page = req.Page,
      PageSize = req.PageSize
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}


