namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, string>
{
  public override void Configure()
  {
    Post(HostRoutes.Base);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Hosts)
      .WithName(DraftsOpenApi.Names.Hosts_CreateHost)
      .Produces<string>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(Features.Permissions.HostCreate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command
    {
      PersonPublicId = req.PersonPublicId
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => HostLocations.ById(created.PublicId),
      ct);
  }
}
