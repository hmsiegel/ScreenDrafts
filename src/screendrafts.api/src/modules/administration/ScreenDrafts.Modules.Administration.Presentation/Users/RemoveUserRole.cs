namespace ScreenDrafts.Modules.Administration.Presentation.Users;

internal sealed class RemoveUserRole(IUsersApi usersApi) : Endpoint<RemoveUserRoleRequest>
{
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Delete("administration/users/{userId:guid}/roles");
    Policies(Presentation.Permissions.ModifyUser);
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Administration)
      .WithDescription("Remove user role")
      .WithName(nameof(RemoveUserRole));
    });
  }

  public override async Task HandleAsync(RemoveUserRoleRequest req, CancellationToken ct)
  {
    var result = await _usersApi.RemoveUserRoleAsync(req.UserId, req.Role);

    await this.MapResultsAsync(result, ct);
  }
}

public record RemoveUserRoleRequest(
  string Role,
  [FromRoute(Name = "userId")] Guid UserId);

internal sealed class RemoveUserRoleSummary : Summary<RemoveUserRole>
{
  public RemoveUserRoleSummary()
  {
    Summary = "Remove user role";
    Description = "Remove user role";
    Response(StatusCodes.Status400BadRequest, "Bad request");
    Response(StatusCodes.Status404NotFound, "User not found");
    Response(StatusCodes.Status403Forbidden, "Forbidden");
  }
}
