namespace ScreenDrafts.Modules.Reporting.Features.Drafts;

internal sealed partial class DraftCompletedIntegrationEventConsumer(
  ISender sender,
  ILogger<DraftCompletedIntegrationEventConsumer> logger
) : IntegrationEventHandler<DraftCompletedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<DraftCompletedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    DraftCompletedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    var command = new MarkDraftCompleteCommand { DraftId = integrationEvent.DraftId };

    var result = await _sender.Send(command, cancellationToken);

    if (result.IsFailure)
    {
      LogFailedToMarkDraftComplete(
        _logger,
        integrationEvent.DraftId,
        string.Join(", ", result.Errors.Select(e => e.Description))
      );
    }
  }

  [LoggerMessage(
    EventId = 1002,
    Level = LogLevel.Error,
    Message = "Failed to mark draft {DraftId} complete in reporting: {Error}"
  )]
  private static partial void LogFailedToMarkDraftComplete(
    ILogger<DraftCompletedIntegrationEventConsumer> logger,
    Guid draftId,
    string error
  );
}
