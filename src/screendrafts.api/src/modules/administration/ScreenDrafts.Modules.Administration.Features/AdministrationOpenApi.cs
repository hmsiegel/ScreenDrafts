namespace ScreenDrafts.Modules.Administration.Features;

internal static class AdministrationOpenApi
{
  public static class Tags
  {
    internal const string Administration = "Administration";
  }

  public static class Names
  {
    // Administration
    public const string Administration_AddPermissionToRole = "Administration.AddPermissionToRole";
    public const string Administration_RemoveUserRole = "Administration.RemoveUserRole";
    public const string Administration_AddUserRole = "Administration.AddUserRole";
    public const string Administration_GetPermissions = "Administration.GetPermissions";
    public const string Administration_GetPermissionsByCode = "Administration.GetPermissionsByCode";
    public const string Administration_GetUserRoles = "Administration.GetUserRoles";
    public const string Administration_AddPermission = "Administration.AddPermission";
    public const string Administration_AddRole = "Administration.AddRole";
  }
}
