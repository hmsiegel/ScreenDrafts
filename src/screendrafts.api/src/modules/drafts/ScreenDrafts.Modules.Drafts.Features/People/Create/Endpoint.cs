namespace ScreenDrafts.Modules.Drafts.Features.People.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, string>
{
  public override void Configure()
  {
    Post(PeopleRoutes.People);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.People)
       .WithName(DraftsOpenApi.Names.People_CreatePerson)
       .Produces<string>(StatusCodes.Status201Created)
       .Produces(StatusCodes.Status400BadRequest)
       .Produces(StatusCodes.Status401Unauthorized)
       .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(Features.Permissions.PersonCreate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command
    {
      UserId = req.UserId,
      FirstName = req.FirstName,
      LastName = req.LastName
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => id.ToString()),
      PeopleLocations.ById,
      ct);
  }
}
