namespace ScreenDrafts.Common.Domain;

public abstract class AggrgateRoot<TId, TIdType> : Entity<TId>
  where TId : AggregateRootId<TIdType>
{
  protected AggrgateRoot(TId id)
    : base(id)
  {
    Id = id;
  }

  protected AggrgateRoot()
  {
  }
}
