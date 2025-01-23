namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.ValueObjects;

public sealed record DrafterId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;
  public static DrafterId CreateUnique() => new(Guid.NewGuid());
  public static DrafterId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
  public static DrafterId Create(Guid value) => new(value);
}
