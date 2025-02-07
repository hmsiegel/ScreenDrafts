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

  public static readonly SDError InvalidName =
    SDError.Failure(
      "UserErrors.InvalidName",
      "The first name and last name must be at least 2 characters long.");

  public static readonly SDError PasswordTooShort =
    SDError.Failure(
      "UserErrors.PasswordTooShort",
      "The password must be at least 6 characters long.");
}
