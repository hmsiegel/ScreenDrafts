namespace ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserEmailChangeRequestedDomainEvent(
  Guid UserId,
  string NewEmail,
  string ConfirmationLink
) : DomainEvent
{
  public Guid UserId { get; } = UserId;
  public string NewEmail { get; } = NewEmail;
  public string ConfirmationLink { get; } = ConfirmationLink;
}
