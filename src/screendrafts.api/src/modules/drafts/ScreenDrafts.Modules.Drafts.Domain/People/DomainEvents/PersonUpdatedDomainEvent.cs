namespace ScreenDrafts.Modules.Drafts.Domain.People.DomainEvents;

public sealed class PersonUpdatedDomainEvent(Guid personId) : DomainEvent
{
  public Guid PersonId { get; } = personId;
}
