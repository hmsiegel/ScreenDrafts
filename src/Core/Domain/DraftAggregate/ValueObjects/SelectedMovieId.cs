namespace ScreenDrafts.Domain.DraftAggregate.ValueObjects;

public sealed class SelectedMovieId : ValueObject
{
    public DefaultIdType Value { get; private set; }

    private SelectedMovieId(DefaultIdType value)
    {
        Value = value;
    }

    public static SelectedMovieId CreateUnique()
    {
        return new SelectedMovieId(DefaultIdType.NewGuid());
    }

    public static SelectedMovieId Create(DefaultIdType value)
    {
        return new SelectedMovieId(value);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

#pragma warning disable CS8618
    private SelectedMovieId()
    {
    }
#pragma warning restore CS8618
}