namespace ScreenDrafts.Modules.Drafts.Features.People.List;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, PeopleCollectionResponse>
{
  public override void Configure()
  {
    Get(PeopleRoutes.People);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.People_ListPeople)
      .WithTags(DraftsOpenApi.Tags.People)
      .Produces<PeopleCollectionResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(Features.Permissions.PersonList);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var query = new Query(req);

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
