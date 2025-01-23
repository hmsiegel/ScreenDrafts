namespace ScreenDrafts.Common.Domain;

public abstract class AggrgateRoot<TId, TIdType> : Entity<TId>
  where TId : AggregateRootId<TIdType>
{
  private readonly List<IDomainEvent> _domainEvents = [];

  protected AggrgateRoot(TId id)
    : base(id)
  {
    Id = id;
  }

  protected AggrgateRoot()
  {
  }

  public IReadOnlyCollection<IDomainEvent> DomainEvents => [.. _domainEvents];

}
