namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StopZoomRecording;

internal sealed class StopZoomRecordingCommandHandler(
  IDraftPartRepository draftPartRepository,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider)
  : ICommandHandler<StopZoomRecordingCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(
    StopZoomRecordingCommand request,
    CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartPublicId,
      cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    if (draftPart.ZoomSessionName is null)
    {
      return Result.Failure(DraftPartErrors.NoActiveZoomSession);
    }

    await _eventBus.PublishAsync(
      new StopZoomRecordingRequestedIntegrationEvent(
        Guid.NewGuid(),
        _dateTimeProvider.UtcNow,
        draftPart.ZoomSessionName,
        draftPart.PublicId),
      cancellationToken);

    return Result.Success();
  }
}
