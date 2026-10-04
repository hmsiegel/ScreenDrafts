namespace ScreenDrafts.Modules.Reporting.Features.Drafts.RecordPartStats;

internal sealed partial class DraftPartStatsRecordedIntegrationEventConsumer(
  ISender sender,
  ILogger<DraftPartStatsRecordedIntegrationEventConsumer> logger
) : IntegrationEventHandler<DraftPartStatsRecordedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<DraftPartStatsRecordedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    DraftPartStatsRecordedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _sender.Send(
      new RecordPartStatsCommand { Stats = integrationEvent },
      cancellationToken
    );

    if (result.IsFailure)
    {
      LogFailedToRecordPartStats(
        _logger,
        integrationEvent.DraftPartPublicId,
        string.Join(", ", result.Errors.Select(e => e.Description))
      );
    }
  }

  [LoggerMessage(
    EventId = 1003,
    Level = LogLevel.Error,
    Message = "Failed to record stats facts for draft part {DraftPartPublicId}: {Error}"
  )]
  private static partial void LogFailedToRecordPartStats(
    ILogger<DraftPartStatsRecordedIntegrationEventConsumer> logger,
    string draftPartPublicId,
    string error
  );
}
