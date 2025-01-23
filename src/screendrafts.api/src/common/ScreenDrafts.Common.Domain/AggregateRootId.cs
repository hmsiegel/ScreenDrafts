namespace ScreenDrafts.Common.Domain;

public abstract record AggregateRootId<TId> : ValueObject
{
  public abstract TId Value { get; protected set; }
}
