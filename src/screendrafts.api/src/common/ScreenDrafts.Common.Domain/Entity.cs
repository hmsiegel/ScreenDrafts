namespace ScreenDrafts.Common.Domain;

public abstract class Entity
{
  private readonly List<IDomainEvent> _domainEvents = [];

  protected Entity(Ulid id)
  {
    Id = id;
  }

  protected Entity()
  {
  }

  public Ulid Id { get; init; } = default!;

  public void ClearDomainEvents()
  {
    _domainEvents.Clear();
  }

  protected void Raise(IDomainEvent domainEvent)
  {
    _domainEvents.Add(domainEvent);
  }
}
