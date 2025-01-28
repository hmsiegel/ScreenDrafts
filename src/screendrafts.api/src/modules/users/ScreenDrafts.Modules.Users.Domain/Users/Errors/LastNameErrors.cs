using ScreenDrafts.Common.Domain;

namespace ScreenDrafts.Modules.Users.Domain.Users.Errors;

public static class LastNameErrors
{
  public static readonly SDError Empty =
    SDError.Problem(
      "LastNameErrors.Empty",
      "Last name is empty.");
  public static readonly SDError TooLong =
    SDError.Problem(
      "LastNameErrors.TooLong",
      "Last name value is too long.");
}
