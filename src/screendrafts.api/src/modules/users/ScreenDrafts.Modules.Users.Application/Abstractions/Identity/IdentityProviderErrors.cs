namespace ScreenDrafts.Modules.Users.Application.Abstractions.Identity;

public static class IdentityProviderErrors
{
  public static readonly SDError EmailIsNotUnique = 
    SDError.Conflict(
      "Identity.EmailIsNotUnique",
      "The specified email is not unique.");
}
