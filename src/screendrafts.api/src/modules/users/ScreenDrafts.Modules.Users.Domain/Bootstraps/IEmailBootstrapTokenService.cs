namespace ScreenDrafts.Modules.Users.Domain.Bootstraps;

public interface IEmailBootstrapTokenService
{
  /// <summary>
  /// Generates a signed, self-verifying token encoding { userId, expiresAt }.
  /// No server-side state is required to validate it — the signature and the
  /// embedded expiry are enough.
  /// </summary>
  string GenerateToken(UserId userId, DateTimeOffset expiresAt);

  /// <summary>
  /// Validates signature and embedded expiry only. Does not check the audit
  /// table (single-use / not-yet-claimed) — callers combine this with a
  /// lookup against IEmailBootstrapClaimRepository.
  /// </summary>
  Result<EmailBootstrapTokenPayload> ValidateToken(string token);
}

public sealed record EmailBootstrapTokenPayload(UserId UserId, DateTimeOffset ExpiresAt);
