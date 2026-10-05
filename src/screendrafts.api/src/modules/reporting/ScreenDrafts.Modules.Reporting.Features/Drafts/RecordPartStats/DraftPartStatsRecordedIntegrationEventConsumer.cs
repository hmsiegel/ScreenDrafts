namespace ScreenDrafts.Modules.Reporting.Features.Drafts.RecordPartStats;

/// <summary>
/// Records the part's stats facts, then gives every credited drafter an honorific appearance.
/// Solo drafters already have one from DraftPartStartedIntegrationEventConsumer (the insert is a
/// no-op for them). Team members get theirs here. Proxy drafters are not credited, so they get none.
/// </summary>
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

    if (integrationEvent.CanonicalPolicyValue == 1)
    {
      return;
    }

    var drafterIds = integrationEvent
      .Picks.SelectMany(p => p.Credits)
      .Select(c => c.DrafterIdValue)
      .Distinct()
      .ToList();

    foreach (var drafterId in drafterIds)
    {
      var honorificResult = await _sender.Send(
        new UpdateDrafterHonorificsCommand
        {
          DrafterIdValue = drafterId,
          DraftId = integrationEvent.DraftId,
          DraftPartPublicId = integrationEvent.DraftPartPublicId,
          CanonicalPolicyValue = integrationEvent.CanonicalPolicyValue,
          HasMainFeedRelease = integrationEvent.HasMainFeedRelease,
        },
        cancellationToken
      );

      if (honorificResult.IsFailure)
      {
        LogFailedToUpdateDrafterHonorifics(
          _logger,
          drafterId,
          integrationEvent.DraftPartPublicId,
          string.Join(", ", honorificResult.Errors.Select(e => e.Description))
        );
      }
    }
  }

  [LoggerMessage(
    EventId = 1004,
    Level = LogLevel.Error,
    Message = "Failed to record stats facts for draft part {DraftPartPublicId}: {Error}"
  )]
  private static partial void LogFailedToRecordPartStats(
    ILogger<DraftPartStatsRecordedIntegrationEventConsumer> logger,
    string draftPartPublicId,
    string error
  );

  [LoggerMessage(
    EventId = 1005,
    Level = LogLevel.Error,
    Message = "Failed to update honorifics for drafter {DrafterId} on part {DraftPartPublicId}: {Error}"
  )]
  private static partial void LogFailedToUpdateDrafterHonorifics(
    ILogger<DraftPartStatsRecordedIntegrationEventConsumer> logger,
    Guid drafterId,
    string draftPartPublicId,
    string error
  );
}
