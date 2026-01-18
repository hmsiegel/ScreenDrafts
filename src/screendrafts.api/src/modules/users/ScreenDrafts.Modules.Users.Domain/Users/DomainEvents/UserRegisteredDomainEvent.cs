namespace ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserRegisteredDomainEvent(Guid UserId) : DomainEvent
{
  public Guid UserId { get; } = UserId;
}
