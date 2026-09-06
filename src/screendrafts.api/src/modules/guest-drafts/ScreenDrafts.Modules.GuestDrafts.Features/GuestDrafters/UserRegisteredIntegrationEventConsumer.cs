using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafters;

internal sealed partial class UserRegisteredIntegrationEventConsumer(
  ISender sender,
  ILogger<UserRegisteredIntegrationEventConsumer> logger
) : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<UserRegisteredIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    UserRegisteredIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _sender.Send(
      new CreateGuestDrafterCommand
      {
        UserId = integrationEvent.UserId,
        FirstName = integrationEvent.FirstName,
        LastName = integrationEvent.LastName,
      },
      cancellationToken
    );

    if (result.IsFailure)
    {
      LogGuestDrafterAlreadyExists(_logger, integrationEvent.UserId, integrationEvent.Id);
    }
  }

  [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Warning,
    Message = "GuestDrafter already exists for user {UserId} (integration event {EventId})"
  )]
  private static partial void LogGuestDrafterAlreadyExists(
    ILogger logger,
    Guid userId,
    Guid eventId
  );
}
