namespace ScreenDrafts.Modules.Integrations.Domain.Zoom;

public sealed class ZoomWebhookPayloadObject
{
  public string AccountId { get; init; } = string.Empty;
  public ZoomWebhookObject? WebhookObject { get; init; }
}
