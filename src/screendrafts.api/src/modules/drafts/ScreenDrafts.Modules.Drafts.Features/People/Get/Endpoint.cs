namespace ScreenDrafts.Modules.Drafts.Features.People.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, PersonResponse>
{
  public override void Configure()
  {
    Get(PeopleRoutes.ById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.People)
       .WithName(DraftsOpenApi.Names.People_GetPersonById)
       .Produces<PersonResponse>(StatusCodes.Status200OK)
       .Produces(StatusCodes.Status404NotFound)
       .Produces(StatusCodes.Status401Unauthorized)
       .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(Features.Permissions.PersonRead);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var query = new Query(req.PublicId);

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
