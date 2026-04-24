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
    DraftPartRecordingId? id = null)
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
    DraftPartRecordingId? id = null)
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
      id: id);
  }
}

public sealed record DraftPartRecordingId(Guid Value)
{
  public Guid Value { get; init; } = Value;
  public static DraftPartRecordingId CreateUnique() => new(Guid.NewGuid());
  public static DraftPartRecordingId FromString(string value) => new(Guid.Parse(value));
  public static DraftPartRecordingId Create(Guid value) => new(value);
}

public sealed class ZoomRecordingFileType(string name, int value)
  : SmartEnum<ZoomRecordingFileType>(name, value)
{
  public static readonly ZoomRecordingFileType Video = new("MP4", 0);
  public static readonly ZoomRecordingFileType Audio = new("M4A", 1);
  public static readonly ZoomRecordingFileType Transcript = new("TRANSCRIPT", 2);
  public static readonly ZoomRecordingFileType Chat = new("CHAT", 3);
  public static readonly ZoomRecordingFileType Timeline = new("TIMELINE", 4);
  public static readonly ZoomRecordingFileType ClosedCaption = new("CC", 5);
}
