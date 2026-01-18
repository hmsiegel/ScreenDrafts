namespace ScreenDrafts.Modules.Users.Domain.Users.Errors;

public static class UserErrors
{
  public static readonly SDError EmailInUse = 
    SDError.Conflict(
      "UserErrors.EmailInUse",
      "The email is already in use.");

  public static SDError NotFound(Guid userId) =>
    SDError.NotFound(
      "UserErrors.NotFound",
      $"The user with id {userId} was not found.");

  public static SDError NotFound(string identityId) =>
    SDError.NotFound(
      "UserErrors.NotFound",
      $"The user with the IDP identity {identityId} was not found.");

  public static SDError PublicIdNotFound (string publicId) =>
    SDError.NotFound(
      "UserErrors.PublicIdNotFound",
      $"The user with public id {publicId} was not found.");

  public static readonly SDError InvalidName =
    SDError.Failure(
      "UserErrors.InvalidName",
      "The first name and last name must be at least 2 characters long.");

  public static readonly SDError PasswordTooShort =
    SDError.Failure(
      "UserErrors.PasswordTooShort",
      "The password must be at least 6 characters long.");

  public static SDError RoleAlreadyExists(Guid userId, string role) =>
    SDError.Problem(
      "UserErrors.RoleAlreadyExists",
      $"The user with id {userId} already has the role {role}.");

  public static readonly SDError RoleDoesNotExist =
    SDError.NotFound(
      "UserErrors.RoleDoesNotExist",
      "The role does not exist.");

  public static readonly SDError CannotRemoveLastRole =
    SDError.Problem(
      "UserErrors.CannotRemoveLastRole",
      "The user must have at least one role.");

  public static SDError PermissionAlreadyExists(string code) =>
    SDError.Problem(
      "UserErrors.PermissionAlreadyExists",
      $"The permission with code {code} already exists.");

  public static SDError PermissionNotFound(string code) =>
    SDError.NotFound(
      "UserErrors.PermissionNotFound",
      $"The permission with code {code} was not found.");
}
