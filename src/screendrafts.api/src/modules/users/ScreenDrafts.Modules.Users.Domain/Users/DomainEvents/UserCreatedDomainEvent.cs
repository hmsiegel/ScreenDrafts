namespace ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserCreatedDomainEvent(Guid userId) : DomainEvent
{
  public Guid UserId { get; } = userId;
}
