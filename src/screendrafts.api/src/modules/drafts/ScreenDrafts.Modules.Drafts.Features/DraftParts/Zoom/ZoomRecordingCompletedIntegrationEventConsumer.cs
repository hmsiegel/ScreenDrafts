namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom;

internal sealed partial class ZoomRecordingCompletedIntegrationEventConsumer(
  IDraftPartRepository draftPartRepository,
  IDraftPartRecordingRepository recordingRepository,
  ILogger<ZoomRecordingCompletedIntegrationEventConsumer> logger,
  IPublicIdGenerator publicIdGenerator)
  : IntegrationEventHandler<ZoomRecordingCompletedIntegrationEvent>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPartRecordingRepository _recordingRepository = recordingRepository;
  private readonly ILogger<ZoomRecordingCompletedIntegrationEventConsumer> _logger = logger;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public override async Task Handle(
    ZoomRecordingCompletedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    var draftPart = await _draftPartRepository.GetByZoomSessionNameAsync(
      integrationEvent.MeetingTopic,
      cancellationToken);

    if (draftPart is null)
    {
      LogDrafPartNotFound(_logger, integrationEvent.MeetingTopic);
      return;
    }

    var existingCount = await _recordingRepository.CountByDraftPartIdAsync(
      draftPartId: draftPart.Id,
      cancellationToken: cancellationToken);

    var newSequenceBase = existingCount;
    var inserted = 0;

    foreach (var file in integrationEvent.RecordingFiles.OrderBy(f => f.RecordingStart))
    {
      var existing = await _recordingRepository.GetByZoomFileIdAsync(
        file.ZoomFileId,
        cancellationToken);

      if (existing is not null)
      {
        LogRecordingFileExists(_logger, file.ZoomFileId);
        continue;
      }

      if (!ZoomRecordingFileType.TryFromName(file.FileType, ignoreCase: true,out var fileType))
      {
        LogUnrecognizedFileType(_logger, file.FileType, file.ZoomFileId);
        continue;
      }

      var recording = DraftPartRecording.Create(
        publicId: _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Recording),
        draftPartId: draftPart.Id,
        downloadUrl: file.DownloadUrl,
        zoomSessionName: integrationEvent.MeetingTopic,
        zoomFileId: file.ZoomFileId,
        fileType: fileType,
        playUrl: file.PlayUrl,
        recordingStart: file.RecordingStart,
        recordingEnd: file.RecordingEnd,
        fileSizeBytes: file.FileSizeBytes,
        sequenceNumber: newSequenceBase + inserted + 1);

      _recordingRepository.Add(recording);
      inserted++;
    }

    if (inserted > 0)
    {
      LogStoredRecordingForZoomSessions(
        _logger,
        inserted,
        draftPart.PublicId,
        integrationEvent.MeetingTopic);
    }
  }

  [LoggerMessage(
    EventId = 1000,
    Level = LogLevel.Warning,
    Message = "No DraftPart was found for Zoom session '{ZoomSessionName}'. Recording event dropped. " +
    "Ensure the session was started via the Screen Drafts app before recording completes.")]
  private static partial void LogDrafPartNotFound(
    ILogger<ZoomRecordingCompletedIntegrationEventConsumer> logger,
    string zoomSessionName);

  [LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Warning,
    Message = "A recording file with Zoom file ID '{ZoomFileId}' already exists. Skipping creation of duplicate recording.")]
  private static partial void LogRecordingFileExists(
    ILogger<ZoomRecordingCompletedIntegrationEventConsumer> logger,
    string zoomFileId);

  [LoggerMessage(
    EventId = 1002,
    Level = LogLevel.Warning,
    Message = "Unrecognized file type '{FileType}' for Zoom file ID '{ZoomFileId}'. Skipping creation of recording.")]
  private static partial void LogUnrecognizedFileType(
    ILogger<ZoomRecordingCompletedIntegrationEventConsumer> logger,
    string fileType,
    string zoomFileId);

  [LoggerMessage(
    EventId = 1003,
    Level = LogLevel.Information,
    Message = "Stored {Inserted} recording(s) for DraftPart '{DraftPartPublicId}' from Zoom session '{ZoomSessionName}'.")]
  private static partial void LogStoredRecordingForZoomSessions(
    ILogger<ZoomRecordingCompletedIntegrationEventConsumer> logger,
    int inserted,
    string draftPartPublicId,
    string zoomSessionName);
}
