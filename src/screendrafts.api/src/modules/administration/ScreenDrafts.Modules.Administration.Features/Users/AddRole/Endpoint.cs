namespace ScreenDrafts.Modules.Administration.Features.Users.AddRole;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddRoleRequest>
{
  public override void Configure()
  {
    Post(UserRoutes.RolesBase);
    Description(x =>
    {
      x.WithName(AdministrationOpenApi.Names.Administration_AddRole)
        .WithTags(AdministrationOpenApi.Tags.Administration)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(AdministrationAuth.Permissions.RoleUpdate);
  }

  public override async Task HandleAsync(AddRoleRequest req, CancellationToken ct)
  {
    var command = new AddRoleCommand
    {
      Name = req.Name
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
