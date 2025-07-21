namespace ScreenDrafts.Modules.Drafts.Domain.People.DomainEvents;

public sealed class PersonCreatedDomainEvent(Guid personId) : DomainEvent
{
  public Guid PersonId { get; } = personId;
}
