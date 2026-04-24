namespace ScreenDrafts.Modules.Integrations.Domain.Zoom;

/// <summary>
/// Zoom sends a URL validation challenge when a webhook endpoint is first registered.
/// The endpoint must respond with a hash of the challenge using the webhook secret token.
/// </summary>
public sealed class ZoomUrlValidationPayload
{
  public string Event { get; init; } = string.Empty;

  public ZoomUrlValidationChallenge? Payload { get; init; }
}
