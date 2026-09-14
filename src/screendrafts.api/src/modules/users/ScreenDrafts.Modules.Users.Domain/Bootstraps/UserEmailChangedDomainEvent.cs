namespace ScreenDrafts.Modules.Users.Domain.Bootstraps;

public sealed class UserEmailChangedDomainEvent(Guid UserId, string NewEmail) : DomainEvent
{
  public Guid UserId { get; } = UserId;
  public string NewEmail { get; } = NewEmail;
}
