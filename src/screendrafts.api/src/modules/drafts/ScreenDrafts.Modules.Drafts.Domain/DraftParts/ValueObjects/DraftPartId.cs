namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

public sealed record DraftPartId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;
  public static DraftPartId CreateUnique() => new(Guid.NewGuid());
  public static DraftPartId FromString(string value) => new(Guid.Parse(value));
  public static DraftPartId Create(Guid value) => new(value);
}
