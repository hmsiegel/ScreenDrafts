namespace ScreenDrafts.Modules.Users.Domain.EmailChange;

public static class EmailChangeErrors
{
  public static readonly SDError SameAsCurrentEmail = SDError.Failure(
    "EmailChange.SameAsCurrentEmail",
    "The new email must be different from your current email."
  );

  public static readonly SDError InvalidToken = SDError.Failure(
    "EmailChange.InvalidToken",
    "This confirmation link is invalid."
  );

  public static readonly SDError AlreadyUsed = SDError.Conflict(
    "EmailChange.AlreadyUsed",
    "This confirmation link has already been used."
  );

  public static readonly SDError Expired = SDError.Failure(
    "EmailChange.Expired",
    "This confirmation link has expired."
  );
}
