namespace ScreenDrafts.Modules.Users.Domain.Bootstraps;

public static class EmailBootstrapClaimErrors
{
  public static SDError NotFound(Guid userId) =>
    SDError.NotFound(
      "EmailBootstrapClaim.NotFound",
      $"No email bootstrap claim exists for user {userId}."
    );

  public static readonly SDError AlreadyClaimed = SDError.Conflict(
    "EmailBootstrapClaim.AlreadyClaimed",
    "This bootstrap token has already been claimed."
  );

  public static readonly SDError Expired = SDError.Failure(
    "EmailBootstrapClaim.Expired",
    "This bootstrap token has expired."
  );

  public static readonly SDError InvalidToken = SDError.Failure(
    "EmailBootstrapClaim.InvalidToken",
    "The token is malformed or its signature is invalid."
  );

  public static readonly SDError CannotReissueClaimed = SDError.Conflict(
    "EmailBootstrapClaim.CannotReissueClaimed",
    "A token cannot be reissued for a user who has already claimed their email."
  );

  public static readonly SDError SocialLoginNotEligible = SDError.Failure(
    "EmailBootstrapClaim.SocialLoginNotEligible",
    "This account uses social login and doesn't need an email migration."
  );
}
