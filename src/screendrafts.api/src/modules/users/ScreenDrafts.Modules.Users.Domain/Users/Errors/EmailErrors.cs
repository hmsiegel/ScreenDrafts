using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Users.Domain.Users.Errors;

public static class EmailErrors
{
  public static readonly SDError Empty =
    SDError.Problem(
      "EmailErrors.Empty",
      "Email is empty.");

  public static readonly SDError TooLong =
    SDError.Problem(
      "EmailErrors.TooLong",
      "Email value is too long.");

  public static readonly SDError Invalid =
    SDError.Problem(
      "EmailErrors.Invalid",
      "Invalid email address.");
}
