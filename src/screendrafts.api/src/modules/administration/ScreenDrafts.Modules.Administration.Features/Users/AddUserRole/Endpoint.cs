namespace ScreenDrafts.Modules.Administration.Features.Users.AddUserRole;

internal sealed class Endpoint(IUsersApi usersApi) : Endpoint<Request>
{
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Post(UserRoutes.UserRoles);
    Description(x =>
    {
      x.WithTags(AdministrationOpenApi.Tags.Administration)
      .WithName(AdministrationOpenApi.Names.Administration_AddUserRole)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(Features.Permissions.UserUpdate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var result = await _usersApi.AddUserRoleAsync(req.UserId, req.Role);

    await this.SendNoContentAsync(result, ct);
  }
}
