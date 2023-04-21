namespace ScreenDrafts.Domain.MovieAggregate.ValueObjects;
public sealed class MovieId : AggregateRootId<DefaultIdType>
{
    public override DefaultIdType Value { get; protected set; }

    private MovieId(DefaultIdType value) => Value = value;

    public static MovieId Create() => new(DefaultIdType.NewGuid());

    public override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    private MovieId()
    {
    }
}
