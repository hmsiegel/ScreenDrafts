using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Administration.Domian;

public static class AdministrationErrors
{
  public static SDError PermissionNotFound(string code) =>
    SDError.NotFound(
        "Administration.PermissionNotFound",
        $"Permission '{code}' does not exist.");

  public static SDError PermissionAlreadyAssigned(string roleName, string code) =>
      SDError.Conflict(
          "Administration.PermissionAlreadyAssigned",
          $"Permission '{code}' is already assigned to role '{roleName}'.");

  public static SDError UserHasNoRoles(Guid userId) =>
      SDError.NotFound(
          "Administration.UserHasNoRoles",
          $"No roles found for user '{userId}'.");

  public static SDError UserNotFound(string publicId) =>
      SDError.NotFound(
          "Administration.UserNotFound",
          $"No user found with public ID '{publicId}'.");

  public static SDError PermissionAlreadyExists(string code) =>
      SDError.Conflict(
          "Administration.PermissionAlreadyExists",
          $"Permission with code '{code}' already exists.");

  public static SDError RoleNotFound(string roleName) =>
      SDError.NotFound(
          "Administration.RoleNotFound",
          $"Role '{roleName}' does not exist.");

   public static SDError RoleAlreadyExists(string roleName) =>
      SDError.Conflict(
          "Administration.RoleAlreadyExists",
          $"Role '{roleName}' already exists.");
}
