namespace ScreenDrafts.Modules.Drafts.Features.Users;

internal sealed class UserRoleAddedIntegrationEventConsumer(
  IPersonRepository personRepository,
  ISender sender
) : IntegrationEventHandler<UserRoleAddedIntegrationEvent>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly ISender _sender = sender;

  private const string DrafterRole = "Drafter";
  private const string HostRole = "Host";

  public override async Task Handle(
    UserRoleAddedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    if (integrationEvent.RoleName is not (DrafterRole or HostRole))
    {
      return;
    }

    var person = await _personRepository.GetByUserIdAsync(
      integrationEvent.UserId,
      cancellationToken
    );

    if (person is null)
    {
      return;
    }

    if (integrationEvent.RoleName == DrafterRole)
    {
      var command = new CreateDrafterCommand(person.PublicId);
      var result = await _sender.Send(command, cancellationToken);

      if (result.IsFailure)
      {
        var isAlreadyExists = result.Errors.Any(e =>
          e.Code.Contains("AlreadyExists", StringComparison.OrdinalIgnoreCase)
        );

        if (!isAlreadyExists)
        {
          throw new ScreenDraftsException(
            $"Failed to create drafter profile for user with ID {integrationEvent.UserId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}"
          );
        }
      }
    }
    else if (integrationEvent.RoleName == HostRole)
    {
      var command = new CreateHostCommand { PersonPublicId = person.PublicId };

      var result = await _sender.Send(command, cancellationToken);

      if (result.IsFailure)
      {
        var isAlreadyExists = result.Errors.Any(e =>
          e.Code.Contains("AlreadyExists", StringComparison.OrdinalIgnoreCase)
        );

        if (!isAlreadyExists)
        {
          throw new ScreenDraftsException(
            $"Failed to create drafter profile for user with ID {integrationEvent.UserId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}"
          );
        }
      }
    }
  }
}
