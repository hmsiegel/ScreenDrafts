using ScreenDrafts.Common.Presentation.Http;

namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermissionToRole;

internal sealed class Endpoint(IUsersApi usersApi) : ScreenDraftsEndpoint<Request>
{
  private readonly IUsersApi _usersApi = usersApi;

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
    Policies(Features.Permissions.RoleUpdate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var result = await _usersApi.AddPermissionToRoleAsync(req.Role!, req.Permission);

    await this.SendNoContentAsync(result, ct);
  }
}
