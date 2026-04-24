namespace ScreenDrafts.Modules.Integrations.Domain.Zoom;

public sealed class ZoomWebhookRecordingFile
{
  public string Id { get; init; } = string.Empty;

  public string MeetingId { get; init; } = string.Empty;

  public string RecordingStart { get; init; } = string.Empty;

  public string RecordingEnd { get; init; } = string.Empty;

  public string FileType { get; init; } = string.Empty;

  public string FileExtension { get; init; } = string.Empty;

  public long FileSize { get; init; }

  public Uri? PlayUrl { get; init; }

  public Uri? DownloadUrl { get; init; }

  public string Status { get; init; } = string.Empty;

  public string RecordingType { get; init; } = string.Empty;
}
