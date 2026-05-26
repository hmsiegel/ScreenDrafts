using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Users.Domain.Abstractions.Identity;

public static class IdentityProviderErrors
{
  public static readonly SDError EmailIsNotUnique = SDError.Conflict(
    "Identity.EmailIsNotUnique",
    "The specified email is not unique."
  );

  public static readonly SDError InvalidCurrentPassword = SDError.Failure(
    "Identity.InvalidCurrentPassword",
    "The current password is invalid."
  );

  public static readonly SDError PasswordChangeFailed = SDError.Failure(
    "Identity.PasswordChangeFailed",
    "Failed to change the password."
  );
}
