namespace ScreenDrafts.Domain.Common.Contracts;
public abstract class AggregateRoot<TId, TIdType> : Entity<TId>
    where TId : AggregateRootId<TIdType>
{
#pragma warning disable CA1061 // Do not hide base class methods
    public new AggregateRootId<TIdType> Id { get; protected set; }
#pragma warning restore CA1061 // Do not hide base class methods

    protected AggregateRoot(TId id)
    {
        Id = id;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected AggregateRoot()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }
}
