namespace ScreenDrafts.Modules.Users.Features.Admin.AddRoleToUser;

internal sealed class UserRoleAddedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider)
  : DomainEventHandler<UserRoleAddedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(UserRoleAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(domainEvent);

    await _eventBus.PublishAsync(
      new UserRoleAddedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        userId: domainEvent.Id,
        roleName: domainEvent.RoleName),
      cancellationToken);
  }
}
