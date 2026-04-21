namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermissionToRole;

internal sealed class Endpoint() : ScreenDraftsEndpoint<AddPermissionToRoleRequest>
{
  public override void Configure()
  {
    Post(UserRoutes.RolePermissions);
    Description(x =>
    {
      x.WithTags(AdministrationOpenApi.Tags.Administration)
      .WithName(AdministrationOpenApi.Names.Administration_AddPermissionToRole)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(AdministrationAuth.Permissions.PermissionsUpdate);
  }

  public override async Task HandleAsync(
    AddPermissionToRoleRequest req,
    CancellationToken ct)
  {
    var command = new AddPermissionToRoleCommand
    {
      PermissionCode = req.PermissionCode,
      RoleName = req.RoleName
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
