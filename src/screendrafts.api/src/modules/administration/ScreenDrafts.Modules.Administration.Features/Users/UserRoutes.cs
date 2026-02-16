namespace ScreenDrafts.Modules.Administration.Features.Users;

internal static class UserRoutes
{
  public const string Base = "/admin";
  public const string Roles = Base + "/roles/{roleName}";
  public const string UserRoles = Roles + "/users/{userId:guid}";
  public const string RolePermissions = Roles + "/permissions/{permission}";
}
