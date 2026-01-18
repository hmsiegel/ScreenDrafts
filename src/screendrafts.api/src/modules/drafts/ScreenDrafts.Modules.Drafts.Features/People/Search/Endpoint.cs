namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, PeopleSearchResponse>
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
    Permissions(Features.Permissions.PersonSearch);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var query = new Query(req.Search, req.Limit);

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
