namespace ScreenDrafts.Modules.Movies.Domain.Movies.ValueObjects;

public sealed record MovieId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static MovieId CreateUnique() => new(Guid.NewGuid());

  public static MovieId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static MovieId Create(Guid value) => new(value);
}
