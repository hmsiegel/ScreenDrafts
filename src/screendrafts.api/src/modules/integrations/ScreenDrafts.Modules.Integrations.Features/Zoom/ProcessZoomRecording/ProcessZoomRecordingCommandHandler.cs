namespace ScreenDrafts.Modules.Integrations.Features.Zoom.ProcessZoomRecording;

internal sealed class ProcessZoomRecordingCommandHandler(
  IEventBus eventBus,
  IDateTimeProvider timeProvider)
  : ICommandHandler<ProcessZoomRecordingCommand>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _timeProvider = timeProvider;

  public async Task<Result> Handle(ProcessZoomRecordingCommand request, CancellationToken cancellationToken)
  {
    await _eventBus.PublishAsync(
      new ZoomRecordingCompletedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: _timeProvider.UtcNow,
      zoomMeetingId: request.ZoomMeetingId,
      meetingTopic: request.MeetingTopic,
      meetingStartTime: request.MeetingStartTime,
      meetingDurationMinutes: request.MeetingDurationMinutes,
      recordingFiles: request.RecordingFiles),
    cancellationToken);

    return Result.Success();
  }
}
