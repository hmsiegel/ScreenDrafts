namespace ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserLinkedToPersonDomainEvent(
Guid userId,
Guid personId,
string personPublicId) : DomainEvent
{
  public Guid UserId { get; } = userId;
  public Guid PersonId { get; } = personId;
  public string PersonPublicId { get; } = personPublicId;
}
