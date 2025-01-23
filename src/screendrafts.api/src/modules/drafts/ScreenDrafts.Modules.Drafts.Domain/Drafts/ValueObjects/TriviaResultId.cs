namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record TriviaResultId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static TriviaResultId CreateUnique() => new(Guid.NewGuid());

  public static TriviaResultId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static TriviaResultId Create(Guid value) => new(value);
}
