namespace ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserRegisteredDomainEvent(Guid userId) : DomainEvent
{
  public Guid UserId { get; } = userId;
}
