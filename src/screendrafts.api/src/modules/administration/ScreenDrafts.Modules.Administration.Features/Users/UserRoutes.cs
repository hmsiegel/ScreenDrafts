namespace ScreenDrafts.Modules.Administration.Features.Users;

internal static class UserRoutes
{
  public const string Base = "/admin";

  // Role assignment
  public const string RolesBase = Base + "/roles";
  public const string Roles = Base + "/roles/{roleName}";
  public const string UserRoles = Roles + "/users/{publicId}";
  public const string RolePermissions = Roles + "/permissions/{permission}";

  // Queries
  public const string Permissions = Base + "/permissions";
  public const string PermissionByCode = Base + "/permissions/{code}";
  public const string UserRolesList = Base + "/users/{publicId}/roles";
}
