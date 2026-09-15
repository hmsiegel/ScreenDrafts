namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;

internal sealed class EmailBootstrapTokenService(IOptions<EmailBootstrapOptions> options)
  : IEmailBootstrapTokenService
{
  private readonly byte[] _secret = Encoding.UTF8.GetBytes(options.Value.Secret);

  public string GenerateToken(UserId userId, DateTimeOffset expiresAt)
  {
    var payload = $"{userId.Value}|{expiresAt.ToUnixTimeSeconds()}";
    var payloadBytes = Encoding.UTF8.GetBytes(payload);

    var signature = ComputeSignature(payloadBytes);

    return $"{Base64UrlEncode(payloadBytes)}.{Base64UrlEncode(signature)}";
  }

  public Result<EmailBootstrapTokenPayload> ValidateToken(string token)
  {
    if (string.IsNullOrWhiteSpace(token))
    {
      return Result.Failure<EmailBootstrapTokenPayload>(EmailBootstrapClaimErrors.InvalidToken);
    }

    var parts = token.Split('.');
    if (parts.Length != 2)
    {
      return Result.Failure<EmailBootstrapTokenPayload>(EmailBootstrapClaimErrors.InvalidToken);
    }

    byte[] payloadBytes;
    byte[] signatureBytes;

    try
    {
      payloadBytes = Base64UrlDecode(parts[0]);
      signatureBytes = Base64UrlDecode(parts[1]);
    }
    catch (FormatException)
    {
      return Result.Failure<EmailBootstrapTokenPayload>(EmailBootstrapClaimErrors.InvalidToken);
    }

    var expectedSignature = ComputeSignature(payloadBytes);

    if (!CryptographicOperations.FixedTimeEquals(signatureBytes, expectedSignature))
    {
      return Result.Failure<EmailBootstrapTokenPayload>(EmailBootstrapClaimErrors.InvalidToken);
    }

    var payloadParts = Encoding.UTF8.GetString(payloadBytes).Split('|');

    if (
      payloadParts.Length != 2
      || !Guid.TryParse(payloadParts[0], out var userIdValue)
      || !long.TryParse(payloadParts[1], out var expiresAtUnixSeconds)
    )
    {
      return Result.Failure<EmailBootstrapTokenPayload>(EmailBootstrapClaimErrors.InvalidToken);
    }

    var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAtUnixSeconds);

    if (DateTimeOffset.UtcNow > expiresAt)
    {
      return Result.Failure<EmailBootstrapTokenPayload>(EmailBootstrapClaimErrors.Expired);
    }

    return new EmailBootstrapTokenPayload(UserId.Create(userIdValue), expiresAt);
  }

  private byte[] ComputeSignature(byte[] payloadBytes)
  {
    using var hmac = new HMACSHA256(_secret);
    return hmac.ComputeHash(payloadBytes);
  }

  private static string Base64UrlEncode(byte[] bytes) =>
    Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

  private static byte[] Base64UrlDecode(string input)
  {
    var padded = input.Replace('-', '+').Replace('_', '/');

    padded = (padded.Length % 4) switch
    {
      2 => padded + "==",
      3 => padded + "=",
      _ => padded,
    };

    return Convert.FromBase64String(padded);
  }
}
