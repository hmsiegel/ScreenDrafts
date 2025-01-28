namespace ScreenDrafts.Common.Domain;

public abstract class Entity
{
  private readonly List<IDomainEvent> _domainEvents = [];

  protected Entity(Guid id)
  {
    Id = id;
  }

  protected Entity()
  {
  }

  public Guid Id { get; init; } = Guid.Empty;

  public IReadOnlyCollection<IDomainEvent> DomainEvents => [.. _domainEvents];

  public void ClearDomainEvents()
  {
    _domainEvents.Clear();
  }

  protected void Raise(IDomainEvent domainEvent)
  {
    _domainEvents.Add(domainEvent);
  }
}
