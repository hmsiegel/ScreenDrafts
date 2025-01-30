namespace ScreenDrafts.Modules.Users.Application.Users.Commands.RegisterUser;

internal sealed class UserRegisteredDomainEventHandler(ISender sender, IEventBus eventBus)
  : IDomainEventHandler<UserRegisteredDomainEvent>
{
  private readonly ISender _sender = sender;
  private readonly IEventBus _eventBus = eventBus;

  public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
  {
    var result = await _sender.Send(
      new GetUserQuery(notification.UserId),
      cancellationToken);

    if (result.IsFailure)
    {
      throw new ScreenDraftsException(nameof(GetUserQuery), result.Error);
    }

    await _eventBus.PublishAsync(
      new UserRegisteredIntegrationEvent(
        notification.Id,
        notification.OccurredOnUtc,
        result.Value.UserId,
        result.Value.Email,
        result.Value.FirstName,
        result.Value.LastName,
        result.Value.MiddleName),
      cancellationToken);
  }
}
