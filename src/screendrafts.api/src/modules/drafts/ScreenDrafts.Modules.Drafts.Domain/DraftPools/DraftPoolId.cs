namespace ScreenDrafts.Modules.Drafts.Domain.DraftPools;

public sealed record DraftPoolId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static DraftPoolId CreateUnique() => new(Guid.NewGuid());
  public static DraftPoolId Create(Guid value) => new(value);
}
