namespace ScreenDrafts.Modules.Reporting.Features.Drafts;

internal sealed partial class DraftPartReleasedIntegrationEventConsumer(
  ISender sender,
  ILogger<DraftPartReleasedIntegrationEventConsumer> logger
) : IntegrationEventHandler<DraftPartReleaseAddedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<DraftPartReleasedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    DraftPartReleaseAddedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    var command = new UpsertDraftPartReleaseCommand
    {
      DraftId = integrationEvent.DraftId,
      DraftPartPublicId = integrationEvent.DraftPartPublicId,
      ReleaseChannel = integrationEvent.ReleaseChannel,
      ReleaseDate = integrationEvent.ReleaseDate,
      EpisodeNumber = integrationEvent.EpisodeNumber,
    };

    var result = await _sender.Send(command, cancellationToken);

    if (result.IsFailure)
    {
      LogFailedToUpsertDraftPartRelease(
        _logger,
        integrationEvent.DraftId,
        integrationEvent.DraftPartPublicId,
        integrationEvent.ReleaseChannel,
        string.Join("; ", result.Errors.Select(e => e.Description))
      );
    }
  }

  [LoggerMessage(
    EventId = 1003,
    Level = LogLevel.Error,
    Message = "Failed to upsert draft part release for DraftId: {DraftId}, DraftPartPublicId: {DraftPartPublicId}, Release Channel: {ReleaseChannel}, Error: {Error}"
  )]
  private static partial void LogFailedToUpsertDraftPartRelease(
    ILogger<DraftPartReleasedIntegrationEventConsumer> logger,
    Guid draftId,
    string draftPartPublicId,
    string releaseChannel,
    string error
  );
}
