namespace ScreenDrafts.Modules.Administration.Features.Users.GetRolePermissions;

internal sealed record GetRolePermissionsRequest
{
  public string RoleName { get; init; } = default!;
}
