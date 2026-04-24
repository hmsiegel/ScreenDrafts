namespace ScreenDrafts.Modules.Integrations.Infrastructure.Zoom;

public sealed class ZoomSettings
{
  public const string SectionName = "Integrations:Zoom";

  public string BaseAddress { get; set; } = string.Empty;
  /// <summary>
  /// Server-to-server OAuth credentials for Zoom REST API calls
  /// (create sessions, control recording)
  /// </summary>
  public string AccountId { get; set; } = string.Empty;
  public string ClientId { get; set; } = string.Empty;
  public string ClientSecret { get; set; } = string.Empty;

  /// <summary>
  /// Video SDK key and secret for signing participant JWT tokenns.
  /// These come from the Video SDK app on the Zoom Marketplace,
  /// which is separate from the Server-to-Server OAuth app used for REST API calls.
  /// </summary>
  public string VideoSdkKey { get; set; } = string.Empty;
  public string VideoSdkSecret { get; set; } = string.Empty;

  /// <summary>
  /// Webhook secret token for HMAC-SHA256 signature verification of incoming Zoom webhook requests
  /// </summary>
  public string WebhookSecretToken { get; set; } = string.Empty;
}
