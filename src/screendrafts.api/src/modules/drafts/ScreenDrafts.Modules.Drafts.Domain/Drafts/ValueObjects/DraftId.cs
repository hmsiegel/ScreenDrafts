namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record DraftId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static DraftId CreateUnique() => new(Guid.NewGuid());

  public static DraftId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static DraftId Create(Guid value) => new(value);
}
