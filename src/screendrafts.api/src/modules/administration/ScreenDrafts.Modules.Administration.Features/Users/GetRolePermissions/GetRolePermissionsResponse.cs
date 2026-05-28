namespace ScreenDrafts.Modules.Administration.Features.Users.GetRolePermissions;

internal sealed record GetRolePermissionsResponse
{
  public IReadOnlyList<string> Permissions { get; init; } = default!;
}
