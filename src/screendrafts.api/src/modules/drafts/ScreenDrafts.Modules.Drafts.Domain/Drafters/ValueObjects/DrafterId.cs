namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.ValueObjects;

public sealed record DrafterId(Ulid Value) : AggregateRootId<Ulid>
{
  public override Ulid Value { get; protected set; } = Value;
  public static DrafterId CreateUnique() => new(Ulid.NewUlid());
  public static DrafterId FromString(string value) => new(Ulid.Parse(value, CultureInfo.InvariantCulture));
  public static DrafterId Create(Ulid value) => new(value);
}
