namespace ScreenDrafts.Modules.Integrations.IntegrationEvents;

public sealed class ZoomRecordingFileModel(
  string zoomFileId,
  string fileType,
  Uri? playUrl,
  Uri? downloadUrl,
  DateTimeOffset recordingStart,
  DateTimeOffset recordingEnd,
  long fileSizeBytes)
{
  public string ZoomFileId { get; init; } = zoomFileId;

  public string FileType { get; init; } = fileType;

  public Uri? PlayUrl { get; init; } = playUrl;

  public Uri? DownloadUrl { get; init; } = downloadUrl;

  public DateTimeOffset RecordingStart { get; init; } = recordingStart;

  public DateTimeOffset RecordingEnd { get; init; } = recordingEnd;

  public long FileSizeBytes { get; init; } = fileSizeBytes;
}
