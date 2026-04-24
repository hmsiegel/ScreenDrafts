namespace ScreenDrafts.Modules.Integrations.Features.Zoom.ProcessZoomRecording;

internal sealed record ProcessZoomRecordingCommand : ICommand
{
  public required string ZoomMeetingId { get; init; }
  public required string MeetingTopic { get; init; }
  public required DateTimeOffset MeetingStartTime { get; init; }
  public int MeetingDurationMinutes { get; init; }
  public IReadOnlyList<ZoomRecordingFileModel> RecordingFiles { get; init; } = [];
}
