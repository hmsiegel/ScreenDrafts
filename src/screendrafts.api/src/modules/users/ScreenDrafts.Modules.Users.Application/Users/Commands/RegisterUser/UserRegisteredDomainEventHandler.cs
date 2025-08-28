namespace ScreenDrafts.Modules.Users.Application.Users.Commands.RegisterUser;

internal sealed class UserRegisteredDomainEventHandler(ISender sender, IEventBus eventBus)
  : DomainEventHandler<UserRegisteredDomainEvent>
{
  private readonly ISender _sender = sender;
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(
    UserRegisteredDomainEvent domainEvent,
    CancellationToken cancellationToken = default)
  {
    var result = await _sender.Send(
      new GetUserQuery(domainEvent.UserId),
      cancellationToken);

    if (result.IsFailure)
    {
      throw new ScreenDraftsException(nameof(GetUserQuery), result.Error);
    }

    await _eventBus.PublishAsync(
      new UserRegisteredIntegrationEvent(
        domainEvent.Id,
        domainEvent.OccurredOnUtc,
        result.Value.UserId,
        result.Value.Email,
        result.Value.FirstName,
        result.Value.LastName,
        result.Value.MiddleName),
      cancellationToken);
  }
}
