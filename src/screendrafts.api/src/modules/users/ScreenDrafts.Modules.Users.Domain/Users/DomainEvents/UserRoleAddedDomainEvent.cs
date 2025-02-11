namespace ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserRoleAddedDomainEvent(Guid userId, string roleName) : DomainEvent
{
  public Guid UserId { get; init; } = userId;

  public string RoleName { get; init; } = roleName;
}
