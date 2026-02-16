namespace ScreenDrafts.Modules.Drafts.Features.People.List;

internal sealed class Endpoint : ScreenDraftsEndpoint<ListPeopleRequest, PeopleCollectionResponse>
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
    Permissions(DraftsAuth.Permissions.PersonList);
  }

  public override async Task HandleAsync(ListPeopleRequest req, CancellationToken ct)
  {
    var ListPeopleQuery = new ListPeopleQuery(req);

    var result = await Sender.Send(ListPeopleQuery, ct);

    await this.SendOkAsync(result, ct);
  }
}


