namespace ScreenDrafts.Modules.Integrations.Infrastructure.Zoom;

internal sealed class ZoomWebhookSignatureVerifier(IOptions<ZoomSettings> zoomSettings) : IZoomWebhookSignatureVerifier
{
  private readonly ZoomSettings _zoomSettings = zoomSettings.Value;

  public string SecretToken => _zoomSettings.WebhookSecretToken;

  public bool IsValid(string timestamp, string signature, string rawBody)
  {
    if (!long.TryParse(timestamp, out var ts))
    {
      return false;
    }

    var age = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(ts);

    if (age.TotalMinutes > 5)
    {
      return false;
    }

    var message = $"v0:{timestamp}:{rawBody}";
    var keyBytes = Encoding.UTF8.GetBytes(_zoomSettings.WebhookSecretToken);
    var messageBytes = Encoding.UTF8.GetBytes(message);

    using var hmac = new HMACSHA256(keyBytes);
    var hashBytes = hmac.ComputeHash(messageBytes);
    var expected = $"v0={Convert.ToHexString(hashBytes).ToLowerInvariant()}";

    return CryptographicOperations.FixedTimeEquals(
      Encoding.UTF8.GetBytes(expected),
      Encoding.UTF8.GetBytes(signature));
  }
}
