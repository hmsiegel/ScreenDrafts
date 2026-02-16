namespace ScreenDrafts.Modules.Administration.Features.Users.RemoveUserRole;

internal sealed class Endpoint(IUsersApi usersApi) : Endpoint<Request>
{
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Delete(UserRoutes.UserRoles);
    Policies(Features.Permissions.UserUpdate);
    Description(x =>
    {
      x.WithTags(AdministrationOpenApi.Tags.Administration)
      .WithName(AdministrationOpenApi.Names.Administration_RemoveUserRole)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status404NotFound)
      .Produces(StatusCodes.Status401Unauthorized);
    });
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var result = await _usersApi.RemoveUserRoleAsync(req.UserId, req.Role);

    await this.SendNoContentAsync(result, ct);
  }
}
