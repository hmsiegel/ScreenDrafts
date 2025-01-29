namespace ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserProfileUpdatedDomainEvent(Guid UserId, string FirstName, string LastName) : DomainEvent
{
  public Guid UserId { get; } = UserId;

  public string FirstName { get; init; } = FirstName;

  public string LastName { get; init; } = LastName;
}
