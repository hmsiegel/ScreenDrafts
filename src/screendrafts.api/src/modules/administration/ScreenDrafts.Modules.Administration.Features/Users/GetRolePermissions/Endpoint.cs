namespace ScreenDrafts.Modules.Administration.Features.Users.GetRolePermissions;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetRolePermissionsRequest, GetRolePermissionsResponse>
{
  public override void Configure()
  {
    Get(UserRoutes.PermissionsRoles);
    Description(x =>
    {
      x.WithName(AdministrationOpenApi.Names.Administration_GetRolePermissions)
        .WithTags(AdministrationOpenApi.Tags.Administration)
        .Produces<GetRolePermissionsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(AdministrationAuth.Permissions.UserRead);
  }

  public override async Task HandleAsync(GetRolePermissionsRequest req, CancellationToken ct)
  {
    var query = new GetRolePermissionsQuery { RoleName = req.RoleName };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
