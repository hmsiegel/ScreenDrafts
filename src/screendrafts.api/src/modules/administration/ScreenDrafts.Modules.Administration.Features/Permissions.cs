namespace ScreenDrafts.Modules.Administration.Features;

internal static class AdministrationAuth
{
  internal static class Permissions
  {
    // Users
    internal const string UserRead = "users:read";
    internal const string UserUpdate = "users:update";

    // Roles
    internal const string RoleRead = "roles:read";
    internal const string RoleUpdate = "roles:update";

    // Permissions
    internal const string PermissionsRead = "permissions:read";
    internal const string PermissionsUpdate = "permissions:update";
  }
}
