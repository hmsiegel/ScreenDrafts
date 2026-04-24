namespace ScreenDrafts.Modules.Integrations.Domain.Zoom;

public interface IZoomWebhookSignatureVerifier
{
  string SecretToken { get; }

  bool IsValid(string timestamp, string signature, string rawBody);
}
