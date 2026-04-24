namespace ScreenDrafts.Modules.Integrations.Domain.Zoom;

public sealed class ZoomWebhookObject
{
  public string Uuid { get; init; } = string.Empty;

  public long Id { get; init; }

  public string HostId { get; init; } = string.Empty;

  public string Topic { get; init; } = string.Empty;

  public int Type { get; init; }

  public DateTimeOffset StartTime { get; init; }

  public int Duration { get; init; }

  public string Timezone { get; init; } = string.Empty;

  public ZoomWebhookRecordingFiles? RecordingFiles { get; init; }
}
