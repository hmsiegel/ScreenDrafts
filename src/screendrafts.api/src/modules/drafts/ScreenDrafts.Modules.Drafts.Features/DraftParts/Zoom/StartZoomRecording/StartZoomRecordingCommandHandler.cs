namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StartZoomRecording;

internal sealed class StartZoomRecordingCommandHandler(
  IDraftPartRepository draftPartRepository,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider)
  : ICommandHandler<StartZoomRecordingCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(
    StartZoomRecordingCommand request,
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
      new StartZoomRecordingRequestedIntegrationEvent(
        Guid.NewGuid(),
        _dateTimeProvider.UtcNow,
        draftPart.ZoomSessionName,
        draftPart.PublicId),
      cancellationToken);

    return Result.Success();
  }
}
