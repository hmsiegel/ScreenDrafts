namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange;

internal sealed class UserEmailChangedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<UserEmailChangedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    UserEmailChangedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      integrationEvent: new UserEmailChangedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        userId: domainEvent.UserId,
        newEmail: domainEvent.NewEmail
      ),
      cancellationToken: cancellationToken
    );
  }
}
