namespace ScreenDrafts.Modules.Reporting.Features.Drafters;

internal sealed partial class DraftPartStartedIntegrationEventConsumer(
  ISender sender,
  ILogger<DraftPartStartedIntegrationEventConsumer> logger)
  : IntegrationEventHandler<DraftPartStartedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<DraftPartStartedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(DraftPartStartedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
  {
    if (integrationEvent.CanonicalPolicyValue == 1)
    {
      return;
    }

    foreach (var participant in integrationEvent.Participants)
    {
      if (participant.ParticipantKindValue != 0)
      {
        continue;
      }

      var command = new UpdateDrafterHonorificsCommand
      {
        DrafterIdValue = participant.ParticipantIdValue,
        DraftPartPublicId = integrationEvent.DraftPartPublicId,
        CanonicalPolicyValue = integrationEvent.CanonicalPolicyValue,
        HasMainFeedRelease = integrationEvent.HasMainFeedRelease
      };

      var result = await _sender.Send(command, cancellationToken);

      if (result.IsFailure)
      {
        Log_FailedToUpdateDrafterHonorifics(
          _logger,
          participant.ParticipantIdValue,
          integrationEvent.DraftPartPublicId,
          string.Join(", ", result.Errors.Select(e => e.Description)));
      }
    }
  }

  [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Error,
    Message = "Failed to update honorifics for drafter {DrafterId} on part {DraftPartPublicId}: {Error}")]
  private static partial void Log_FailedToUpdateDrafterHonorifics(ILogger logger, Guid drafterId, string draftPartPublicId, string error);
}
