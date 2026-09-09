using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafters;

internal sealed partial class UserNameUpdatedIntegrationEventConsumer(
  ISender sender,
  ILogger<UserNameUpdatedIntegrationEventConsumer> logger
) : IntegrationEventHandler<UserNameUpdatedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<UserNameUpdatedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    UserNameUpdatedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _sender.Send(
      new UpdateDrafterNameCommand
      {
        UserId = integrationEvent.UserId,
        FirstName = integrationEvent.FirstName,
        LastName = integrationEvent.LastName,
      },
      cancellationToken
    );

    if (result.IsFailure)
    {
      LogFailedToUpdateGuestDrafterName(_logger, integrationEvent.UserId, integrationEvent.Id);
    }
  }

  [LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Warning,
    Message = "Failed to update GuestDrafter name for user {UserId} (integration event {EventId})"
  )]
  private static partial void LogFailedToUpdateGuestDrafterName(
    ILogger logger,
    Guid userId,
    Guid eventId
  );
}
