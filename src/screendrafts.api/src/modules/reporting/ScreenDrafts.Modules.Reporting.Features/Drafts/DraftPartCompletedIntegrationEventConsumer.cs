namespace ScreenDrafts.Modules.Reporting.Features.Drafts;

internal sealed partial class DraftPartCompletedIntegrationEventConsumer(
  ISender sender,
  ILogger<DraftPartCompletedIntegrationEventConsumer> logger
) : IntegrationEventHandler<DraftPartCompletedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<DraftPartCompletedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    DraftPartCompletedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    var command = new UpsertDraftSummaryCommand
    {
      DraftId = integrationEvent.DraftId,
      DraftPublicId = integrationEvent.DraftPublicId,
      DraftPartPublicId = integrationEvent.DraftPartPublicId,
      Title = integrationEvent.Title,
      DraftType = integrationEvent.DraftType,
      PartIndex = integrationEvent.PartIndex,
      TotalParts = integrationEvent.TotalParts,
      IsPatreon = integrationEvent.IsPatreon,
      EpisodeNumber = integrationEvent.EpisodeNumber,
      VetoCount = integrationEvent.VetoCount,
    };

    var result = await _sender.Send(command, cancellationToken);

    if (result.IsFailure)
    {
      LogFailedToUpsertDraftSummary(
        logger: _logger,
        draftId: integrationEvent.DraftId,
        error: string.Join(", ", result.Errors.Select(e => e.Description))
      );
    }
  }

  [LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Error,
    Message = "Failed to upsert draft summary for DraftId: {DraftId}, Error: {Error}"
  )]
  private static partial void LogFailedToUpsertDraftSummary(
    ILogger<DraftPartCompletedIntegrationEventConsumer> logger,
    Guid draftId,
    string error
  );
}
