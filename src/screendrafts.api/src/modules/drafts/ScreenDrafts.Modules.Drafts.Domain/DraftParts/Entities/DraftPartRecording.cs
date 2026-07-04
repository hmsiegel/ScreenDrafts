namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class DraftPartRecording : Entity<DraftPartRecordingId>
{
  private DraftPartRecording(
    string publicId,
    DraftPartId draftPartId,
    string zoomSessionName,
    string zoomFileId,
    ZoomRecordingFileType fileType,
    Uri? playUrl,
    Uri? downloadUrl,
    DateTimeOffset recordingStart,
    DateTimeOffset recordingEnd,
    long fileSizeBytes,
    int sequenceNumber,
    DraftPartRecordingId? id = null
  )
    : base(id ?? DraftPartRecordingId.CreateUnique())
  {
    PublicId = publicId;
    DraftPartId = draftPartId;
    ZoomSessionName = zoomSessionName;
    ZoomFileId = zoomFileId;
    FileType = fileType;
    PlayUrl = playUrl;
    DownloadUrl = downloadUrl;
    RecordingStart = recordingStart;
    RecordingEnd = recordingEnd;
    FileSizeBytes = fileSizeBytes;
    SequenceNumber = sequenceNumber;
  }

  private DraftPartRecording() { }

  public string PublicId { get; private set; } = default!;
  public DraftPartId DraftPartId { get; private set; } = default!;
  public string ZoomSessionName { get; private set; } = default!;
  public string ZoomFileId { get; private set; } = default!;
  public ZoomRecordingFileType FileType { get; private set; } = ZoomRecordingFileType.Video;
  public Uri? PlayUrl { get; private set; }
  public Uri? DownloadUrl { get; private set; }
  public DateTimeOffset RecordingStart { get; private set; }
  public DateTimeOffset RecordingEnd { get; private set; }
  public long FileSizeBytes { get; private set; }
  public int SequenceNumber { get; private set; }

  public static DraftPartRecording Create(
    string publicId,
    DraftPartId draftPartId,
    string zoomSessionName,
    string zoomFileId,
    ZoomRecordingFileType fileType,
    Uri? playUrl,
    Uri? downloadUrl,
    DateTimeOffset recordingStart,
    DateTimeOffset recordingEnd,
    long fileSizeBytes,
    int sequenceNumber,
    DraftPartRecordingId? id = null
  )
  {
    return new DraftPartRecording(
      publicId: publicId,
      draftPartId: draftPartId,
      zoomSessionName: zoomSessionName,
      zoomFileId: zoomFileId,
      fileType: fileType,
      playUrl: playUrl,
      downloadUrl: downloadUrl,
      recordingStart: recordingStart,
      recordingEnd: recordingEnd,
      fileSizeBytes: fileSizeBytes,
      sequenceNumber: sequenceNumber,
      id: id
    );
  }
}
