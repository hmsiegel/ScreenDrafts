namespace ScreenDrafts.Modules.Integrations.Domain.Zoom;

public sealed class ZoomWebhookPayload
{
  public string Event { get; init; } = string.Empty;
  public string EventTs { get; init; } = string.Empty;
  public ZoomWebhookPayloadObject? Payload { get; init; }
}
