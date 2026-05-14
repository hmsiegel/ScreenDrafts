namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermissionToRole;

internal sealed record AddPermissionToRoleRequest
{
  [FromRoute(Name = "permission")]
  public string PermissionCode { get; init; } = default!;

  [FromRoute(Name = "roleName")]
  public string RoleName { get; init; } = default!;
}

