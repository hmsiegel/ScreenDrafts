namespace ScreenDrafts.Modules.Users.Features.Users.Update;

internal sealed class UserProfileUpdatedDomainEventHandler(
  IUserRepository userRepository,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<UserProfileUpdatedDomainEvent>
{
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    UserProfileUpdatedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    var userId = UserId.Create(domainEvent.UserId);

    var user = await _userRepository.GetAsync(userId, cancellationToken);

    if (user is null)
    {
      return;
    }

    if (user.FirstName.Value is null || user.LastName.Value is null)
    {
      return;
    }

    await _eventBus.PublishAsync(
      integrationEvent: new UserNameUpdatedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        userId: userId.Value,
        firstName: user.FirstName.Value,
        lastName: user.LastName.Value,
        middleName: user.MiddleName
      ),
      cancellationToken: cancellationToken
    );
  }
}
