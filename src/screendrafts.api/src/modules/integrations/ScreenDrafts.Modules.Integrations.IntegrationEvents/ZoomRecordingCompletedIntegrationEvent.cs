using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Integrations.IntegrationEvents;

public sealed class ZoomRecordingCompletedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  string zoomMeetingId,
  string meetingTopic,
  DateTimeOffset meetingStartTime,
  int meetingDurationMinutes,
  IReadOnlyList<ZoomRecordingFileModel> recordingFiles)
  : IntegrationEvent(id, occurredOnUtc)
{

  /// <summary>
  /// The Zoom meeting UUID from the webhook envelope.
  /// Kept for audit/traceability but not used for DraftPart correlation.
  /// </summary>
  public string ZoomMeetingId { get; init; } = zoomMeetingId;

  /// <summary>
  /// The session name set by ScreenDrafts when the host started the session.
  /// Zoom echoes it back as the meeting topic. Used to correlate the recording
  /// back to the correct DraftPart.
  /// </summary>
  public string MeetingTopic { get; init; } = meetingTopic;

  public DateTimeOffset MeetingStartTime { get; init; } = meetingStartTime;

  public int MeetingDurationMinutes { get; init; } = meetingDurationMinutes;

  public IReadOnlyList<ZoomRecordingFileModel> RecordingFiles { get; init; } = recordingFiles;
}
