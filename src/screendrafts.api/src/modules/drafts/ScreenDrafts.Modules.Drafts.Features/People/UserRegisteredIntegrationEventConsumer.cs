namespace ScreenDrafts.Modules.Drafts.Features.People;

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
      new Create.Command
      {
        FirstName = integrationEvent.FirstName,
        LastName = integrationEvent.LastName,
        UserId = integrationEvent.UserId
      },
      cancellationToken);

    if (result.IsFailure)
    {
      PeogleLoggingMessages.PersonAlreadyExists(
        _logger,
        integrationEvent.Id);
    }
  }
}
