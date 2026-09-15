namespace ScreenDrafts.Modules.Users.Domain.Bootstraps;

/// <summary>
/// Audit row for the one-time fake-to-real email migration. One row per user.
/// The token itself is a self-verifying signed HMAC (see IEmailBootstrapTokenService) —
/// this row exists so a token can be single-use and so a batch can be reissued
/// (fresh expiry) without waiting for the old one to lapse.
/// </summary>
public sealed class EmailBootstrapClaim
{
  private EmailBootstrapClaim(
    Guid id,
    UserId userId,
    DateTimeOffset issuedAt,
    DateTimeOffset expiresAt,
    string? batchLabel
  )
  {
    Id = id;
    UserId = userId;
    IssuedAt = issuedAt;
    ExpiresAt = expiresAt;
    BatchLabel = batchLabel;
  }

  private EmailBootstrapClaim() { }

  public Guid Id { get; private set; }
  public UserId UserId { get; private set; } = default!;
  public DateTimeOffset IssuedAt { get; private set; }
  public DateTimeOffset ExpiresAt { get; private set; }
  public string? BatchLabel { get; private set; }
  public DateTimeOffset? ClaimedAt { get; private set; }
  public string? ClaimedEmail { get; private set; }

  public bool IsClaimed => ClaimedAt is not null;

  public static EmailBootstrapClaim Issue(
    UserId userId,
    DateTimeOffset issuedAt,
    DateTimeOffset expiresAt,
    string? batchLabel
  ) => new(Guid.NewGuid(), userId, issuedAt, expiresAt, batchLabel);

  /// <summary>
  /// Mints a fresh window for a user who hasn't claimed yet — used when a later
  /// batch (e.g. the Discord round) needs to reach people the first batch didn't.
  /// </summary>
  public Result Reissue(DateTimeOffset issuedAt, DateTimeOffset expiresAt, string? batchLabel)
  {
    if (IsClaimed)
    {
      return Result.Failure(EmailBootstrapClaimErrors.CannotReissueClaimed);
    }

    IssuedAt = issuedAt;
    ExpiresAt = expiresAt;
    BatchLabel = batchLabel;

    return Result.Success();
  }

  public Result Claim(string claimedEmail, DateTimeOffset claimedAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(claimedEmail);

    if (IsClaimed)
    {
      return Result.Failure(EmailBootstrapClaimErrors.AlreadyClaimed);
    }

    if (claimedAt > ExpiresAt)
    {
      return Result.Failure(EmailBootstrapClaimErrors.Expired);
    }

    ClaimedAt = claimedAt;
    ClaimedEmail = claimedEmail;

    return Result.Success();
  }
}
