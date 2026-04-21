namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermissionToRole;

internal sealed record AddPermissionToRoleRequest
{
  [FromRoute(Name = "permission")]
  public required string PermissionCode { get; init; }

  [FromRoute(Name = "roleName")]
  public required string RoleName { get; init; }
}

