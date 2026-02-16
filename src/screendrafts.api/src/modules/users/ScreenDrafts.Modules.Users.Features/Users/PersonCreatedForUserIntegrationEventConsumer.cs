namespace ScreenDrafts.Modules.Users.Features.Users;

internal static partial class PersonCreatedForUserIntegrationEventConsumerLogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Failed to update person for user with ID {UserId}. Errors: {Errors}")]
    public static partial void LogFailedToUpdatePerson(
        ILogger logger,
        Guid userId,
        string errors);
}

internal sealed class PersonCreatedForUserIntegrationEventConsumer(
  ISender sender,
  ILogger<PersonCreatedForUserIntegrationEventConsumer> logger)
  : IntegrationEventHandler<PersonCreatedForUserIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<PersonCreatedForUserIntegrationEventConsumer> _logger = logger;
  public override async Task Handle(
    PersonCreatedForUserIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    var command = new UpdatePerson.UpdatePersonCommand
    {
      UserId = integrationEvent.UserId,
      PersonId = integrationEvent.PersonId,
      PersonPublicId = integrationEvent.PersonPublicId
    };

    var result = await _sender.Send(command, cancellationToken);

    if (result.IsFailure)
    {
      PersonCreatedForUserIntegrationEventConsumerLogMessages.LogFailedToUpdatePerson(
        _logger,
        integrationEvent.UserId,
        string.Join(", ", result.Errors.Select(e => e.Description)));
    }
  }
}
