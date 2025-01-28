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
}
