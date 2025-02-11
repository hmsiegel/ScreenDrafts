namespace ScreenDrafts.Modules.Administration.Presentation.Users;

internal sealed class AddPermissionToRole(IUsersApi usersApi) : Endpoint<AddPermissionToRoleRequest>
{
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Post("administration/roles/{role}/permissions");
    Policies(Presentation.Permissions.ModifyRole);
    Description(x => x.WithTags(Presentation.Tags.Administration));
  }

  public override async Task HandleAsync(AddPermissionToRoleRequest req, CancellationToken ct)
  {
    var role = Route<string>("role");

    var result = await _usersApi.AddPermissionToRoleAsync(role!, req.Permission);

    if (result.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await SendOkAsync(StatusCodes.Status200OK, ct);
    }
  }
}

internal sealed record AddPermissionToRoleRequest(string Permission);
