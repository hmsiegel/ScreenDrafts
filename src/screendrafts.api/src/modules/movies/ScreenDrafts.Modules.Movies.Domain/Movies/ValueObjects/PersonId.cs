namespace ScreenDrafts.Modules.Movies.Domain.Movies.ValueObjects;

public sealed record PersonId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static PersonId CreateUnique() => new(Guid.NewGuid());

  public static PersonId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static PersonId Create(Guid value) => new(value);
}
