namespace ScreenDrafts.Domain.MovieAggregate.ValueObjects;
public sealed class MovieId : AggregateRootId<DefaultIdType>
{
    public override DefaultIdType Value { get; protected set; }

    private MovieId(DefaultIdType value) => Value = value;

    public static MovieId CreateUnique() => new(DefaultIdType.NewGuid());
    public static MovieId Create(DefaultIdType value) => new(value);

    public override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    private MovieId()
    {
    }
}
