using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreateHostRequest, string>
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
    Policies(DraftsAuth.Permissions.HostCreate);
  }

  public override async Task HandleAsync(CreateHostRequest req, CancellationToken ct)
  {
    var CreateHostCommand = new CreateHostCommand
    {
      PersonPublicId = req.PersonPublicId
    };

    var result = await Sender.Send(CreateHostCommand, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => HostLocations.ById(created.PublicId),
      ct);
  }
}


