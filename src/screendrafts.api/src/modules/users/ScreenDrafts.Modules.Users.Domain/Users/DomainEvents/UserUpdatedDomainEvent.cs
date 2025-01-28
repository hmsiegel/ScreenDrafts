namespace ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserUpdatedDomainEvent(Guid UserId) : DomainEvent
{
  public Guid UserId { get; } = UserId;
}
