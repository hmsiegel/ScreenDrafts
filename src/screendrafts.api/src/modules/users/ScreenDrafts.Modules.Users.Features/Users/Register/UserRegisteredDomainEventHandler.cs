namespace ScreenDrafts.Modules.Users.Features.Users.Register;

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
      new GetByUserId.GetByUserIdQuery(domainEvent.UserId),
      cancellationToken);

    if (result.IsFailure)
    {
      throw new ScreenDraftsException(nameof(GetByUserId.GetByUserIdQuery), result.Error);
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
      cancellationToken: cancellationToken);
  }
}
