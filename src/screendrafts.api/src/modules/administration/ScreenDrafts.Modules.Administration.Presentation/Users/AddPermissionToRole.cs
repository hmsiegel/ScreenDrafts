using ScreenDrafts.Common.Presentation.Results;

namespace ScreenDrafts.Modules.Administration.Presentation.Users;

internal sealed class AddPermissionToRole(IUsersApi usersApi) : Endpoint<AddPermissionToRoleRequest>
{
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Post("administration/roles/{role}/permissions");
    Policies(Presentation.Permissions.ModifyRole);
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Administration)
      .WithDescription("Add permission to role")
      .WithName(nameof(AddPermissionToRole));
    });
  }

  public override async Task HandleAsync(AddPermissionToRoleRequest req, CancellationToken ct)
  {
    var result = await _usersApi.AddPermissionToRoleAsync(req.Role!, req.Permission);

    await this.SendNoContentAsync(result, ct);
  }
}

internal sealed record AddPermissionToRoleRequest(
  string Permission,
  [FromRoute(Name = "role")] string Role);

internal sealed class AddPermissionToRoleSummary : Summary<AddPermissionToRole>
{
  public AddPermissionToRoleSummary()
  {
    Summary = "Add permission to role";
    Description = "Add permission to role";
    Response<ErrorResponse>(StatusCodes.Status400BadRequest, "Bad request");
    Response<ErrorResponse>(StatusCodes.Status404NotFound, "Not found");
    Response<ErrorResponse>(StatusCodes.Status500InternalServerError, "Internal server error");
  }
}
