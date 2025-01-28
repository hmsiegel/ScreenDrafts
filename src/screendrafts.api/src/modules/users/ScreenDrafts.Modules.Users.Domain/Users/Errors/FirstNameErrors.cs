using ScreenDrafts.Common.Domain;

namespace ScreenDrafts.Modules.Users.Domain.Users.Errors;

public static class FirstNameErrors
{
  public static readonly SDError Empty =
    SDError.Problem(
      "FirstNameErrors.Empty",
      "First name is empty.");
  public static readonly SDError TooLong =
    SDError.Problem(
      "FirstNameErrors.TooLong",
      "First name value is too long.");
}
