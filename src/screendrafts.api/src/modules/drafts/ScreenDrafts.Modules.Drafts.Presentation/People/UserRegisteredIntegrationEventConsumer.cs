namespace ScreenDrafts.Modules.Drafts.Presentation.People;

internal sealed class UserRegisteredIntegrationEventConsumer(
  ISender sender,
  ILogger<UserRegisteredIntegrationEventConsumer> logger)
    : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<UserRegisteredIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    UserRegisteredIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    var result = await _sender.Send(
      new CreatePersonCommand(
        integrationEvent.FirstName,
        integrationEvent.LastName,
        integrationEvent.UserId),
            cancellationToken);

    if (result.IsFailure)
    {
      PeogleLoggingMessages.PersonAlreadyExists(
        _logger,
        integrationEvent.Id);
    }
  }
}
