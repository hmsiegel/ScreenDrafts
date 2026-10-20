namespace ScreenDrafts.Modules.Communications.Domain.ContactMessages;

/// <summary>
/// Verifies a Cloudflare Turnstile token server-side. Fails closed: any problem
/// reaching Cloudflare, or a missing secret, returns false.
/// </summary>
public interface ITurnstileVerifier
{
  Task<bool> VerifyAsync(
    string token,
    string? remoteIpAddress,
    CancellationToken cancellationToken = default
  );
}
