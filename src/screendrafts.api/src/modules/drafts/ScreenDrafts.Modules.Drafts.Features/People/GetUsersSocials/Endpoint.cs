namespace ScreenDrafts.Modules.Drafts.Features.People.GetUsersSocials;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, Response>
{
  public override void Configure()
  {
    Post(PeopleRoutes.PublicProfiles);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.People)
        .WithName(DraftsOpenApi.Names.People_GetUsersSocials)
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.PersonProfile);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var query = new GetUsersSocialsQuery { PersonIds = req.PublicIds };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
