namespace ScreenDrafts.Modules.Administration.Features.Users.GetRolePermissions;

internal sealed record GetRolePermissionsQuery : IQuery<GetRolePermissionsResponse>
{
  public string RoleName { get; init; } = default!;
}
