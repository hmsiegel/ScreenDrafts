namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermissionToRole;

internal sealed record AddPermissionToRoleCommand : ICommand<bool>
{
  public required string PermissionCode { get; init; }

  public required string RoleName { get; init; }
}
