namespace ScreenDrafts.Modules.Drafts.Domain.People.DomainEvents;

public sealed class PersonCreatedDomainEvent(
  Guid personId,
  string publicId) : DomainEvent
{
  public Guid PersonId { get; } = personId;
  public string PublicId { get; } = publicId;
}
