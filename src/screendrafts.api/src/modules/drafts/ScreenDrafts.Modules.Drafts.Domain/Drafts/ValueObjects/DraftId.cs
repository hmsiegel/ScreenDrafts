namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record DraftId(Ulid Value) : AggregateRootId<Ulid>
{
  public override Ulid Value { get; protected set; } = Value;

  public static DraftId CreateUnique() => new(Ulid.NewUlid());

  public static DraftId FromString(string value) => new(Ulid.Parse(value, CultureInfo.InvariantCulture));

  public static DraftId Create(Ulid value) => new(value);
}
