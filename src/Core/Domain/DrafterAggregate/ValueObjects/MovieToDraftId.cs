namespace ScreenDrafts.Domain.DrafterAggregate.ValueObjects;
public sealed class MovieToDraftId : ValueObject
{
    public DefaultIdType Value { get; private set; }

    private MovieToDraftId(DefaultIdType value)
    {
        Value = value;
    }

    public static MovieToDraftId CreateUnique()
    {
        return new MovieToDraftId(DefaultIdType.NewGuid());
    }

    public static MovieToDraftId Create(DefaultIdType value)
    {
        return new MovieToDraftId(value);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

#pragma warning disable CS8618
    private MovieToDraftId()
    {
    }
#pragma warning restore CS8618
}
