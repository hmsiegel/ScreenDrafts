namespace ScreenDrafts.Modules.Drafts.Features.People;

internal sealed partial class UserNameUpdatedIntegrationEventConsumer(
  IPersonRepository personRepository,
  ILogger<UserNameUpdatedIntegrationEventConsumer> logger
) : IntegrationEventHandler<UserNameUpdatedIntegrationEvent>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly ILogger<UserNameUpdatedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    UserNameUpdatedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    var person = await _personRepository.GetByUserIdAsync(
      integrationEvent.UserId,
      cancellationToken
    );

    if (person is null)
    {
      return;
    }

    var autoName = $"{person.FirstName} {person.LastName}".Trim();
    var displayName = person.DisplayName == autoName ? null : person.DisplayName;

    var result = person.Update(integrationEvent.FirstName, integrationEvent.LastName, displayName);

    if (result.IsFailure)
    {
      LogFailedToUpdatePerson(_logger, integrationEvent.UserId, integrationEvent.Id);
      return;
    }

    _personRepository.Update(person);
  }

  [LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Error,
    Message = "Failed to update person with UserId {UserId} for UserNameUpdatedIntegrationEvent with Id {IntegrationEventId}."
  )]
  private static partial void LogFailedToUpdatePerson(
    ILogger<UserNameUpdatedIntegrationEventConsumer> logger,
    Guid userId,
    Guid integrationEventId
  );
}
