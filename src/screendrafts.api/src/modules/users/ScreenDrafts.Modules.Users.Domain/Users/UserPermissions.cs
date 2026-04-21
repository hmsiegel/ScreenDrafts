namespace ScreenDrafts.Modules.Users.Domain.Users;

public sealed class UserPermissions
{
  public Guid UserId { get; init; }
  public string PermissionCode { get; init; } = string.Empty;
}
