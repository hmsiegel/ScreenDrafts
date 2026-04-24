namespace ScreenDrafts.Modules.Integrations.Domain.Zoom;

public sealed class ZoomWebhookRecordingFiles
{
  public IReadOnlyList<ZoomWebhookRecordingFile> Files { get; init; } = [];
}
