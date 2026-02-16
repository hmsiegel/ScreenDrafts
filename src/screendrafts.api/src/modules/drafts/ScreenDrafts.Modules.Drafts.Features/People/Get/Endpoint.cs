namespace ScreenDrafts.Modules.Drafts.Features.People.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetPersonRequest, PersonResponse>
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
    Permissions(DraftsAuth.Permissions.PersonRead);
  }

  public override async Task HandleAsync(GetPersonRequest req, CancellationToken ct)
  {
    var GetPersonQuery = new GetPersonQuery(req.PublicId);

    var result = await Sender.Send(GetPersonQuery, ct);

    await this.SendOkAsync(result, ct);
  }
}


