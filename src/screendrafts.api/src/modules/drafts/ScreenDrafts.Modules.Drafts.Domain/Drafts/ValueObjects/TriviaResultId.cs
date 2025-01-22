namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record TriviaResultId(Ulid Value) : AggregateRootId<Ulid>
{
  public override Ulid Value { get; protected set; } = Value;

  public static TriviaResultId CreateUnique() => new(Ulid.NewUlid());

  public static TriviaResultId FromString(string value) => new(Ulid.Parse(value, CultureInfo.InvariantCulture));

  public static TriviaResultId Create(Ulid value) => new(value);
}
