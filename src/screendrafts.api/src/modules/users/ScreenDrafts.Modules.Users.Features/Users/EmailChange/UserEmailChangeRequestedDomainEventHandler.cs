namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange;

internal sealed class UserEmailChangeRequestedDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<UserEmailChangeRequestedDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    UserEmailChangeRequestedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await _eventBus.PublishAsync(
      integrationEvent: new EmailChangeConfirmationRequestedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        userId: domainEvent.UserId,
        newEmail: domainEvent.NewEmail,
        confirmationLink: domainEvent.ConfirmationLink
      ),
      cancellationToken: cancellationToken
    );
  }
}
