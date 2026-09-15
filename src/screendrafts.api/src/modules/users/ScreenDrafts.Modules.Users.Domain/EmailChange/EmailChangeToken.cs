namespace ScreenDrafts.Modules.Users.Domain.EmailChange;

/// <summary>
/// Steady-state (real -> real) email change token. Unlike the bootstrap token,
/// this one is not self-verifying — it's a random value whose SHA-256 hash is
/// stored here, so it can be positively invalidated (on use, or when superseded
/// by a newer request) rather than just left to expire.
/// </summary>
public sealed class EmailChangeToken
{
  private EmailChangeToken(
    Guid id,
    UserId userId,
    string tokenHash,
    string newEmail,
    DateTimeOffset issuedAt,
    DateTimeOffset expiresAt
  )
  {
    Id = id;
    UserId = userId;
    TokenHash = tokenHash;
    NewEmail = newEmail;
    IssuedAt = issuedAt;
    ExpiresAt = expiresAt;
  }

  private EmailChangeToken() { }

  public Guid Id { get; private set; }
  public UserId UserId { get; private set; } = default!;
  public string TokenHash { get; private set; } = default!;
  public string NewEmail { get; private set; } = default!;
  public DateTimeOffset IssuedAt { get; private set; }
  public DateTimeOffset ExpiresAt { get; private set; }
  public DateTimeOffset? UsedAt { get; private set; }

  public bool IsUsed => UsedAt is not null;

  public static EmailChangeToken Issue(
    UserId userId,
    string newEmail,
    string tokenHash,
    DateTimeOffset issuedAt,
    DateTimeOffset expiresAt
  ) => new(Guid.NewGuid(), userId, tokenHash, newEmail, issuedAt, expiresAt);

  public Result MarkUsed(DateTimeOffset usedAt)
  {
    if (IsUsed)
    {
      return Result.Failure(EmailChangeErrors.AlreadyUsed);
    }

    UsedAt = usedAt;

    return Result.Success();
  }

  /// <summary>
  /// Supersedes a pending token — called when the user requests another change
  /// before confirming this one, so only one token is ever valid at a time.
  /// No-op if already used/invalidated.
  /// </summary>
  public void Invalidate(DateTimeOffset invalidatedAt)
  {
    if (IsUsed)
    {
      return;
    }

    UsedAt = invalidatedAt;
  }
}
