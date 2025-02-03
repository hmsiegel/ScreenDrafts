namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

public sealed class UserRegisteredIntegrationEventHandler(ISender sender)
  : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
  private readonly ISender _sender = sender;

  public override async Task Handle(
    UserRegisteredIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    var drafterName = $"{integrationEvent.FirstName} {integrationEvent.MiddleName} {integrationEvent.LastName}";

    var command = new CreateDrafterCommand(drafterName, integrationEvent.UserId);

    var result = await _sender.Send(
      command,
      cancellationToken);

    if (result.IsFailure)
    {
      throw new ScreenDraftsException(nameof(CreateDrafterCommand), result.Error);
    }
  }
}
