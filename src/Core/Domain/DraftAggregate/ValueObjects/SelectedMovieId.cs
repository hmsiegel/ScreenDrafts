namespace ScreenDrafts.Domain.DraftAggregate.ValueObjects;

public sealed class SelectedMovieId : ValueObject
{
    public Guid Value { get; private set; }

    private SelectedMovieId(Guid value)
    {
        Value = value;
    }

    public static SelectedMovieId CreateUnique()
    {
        return new SelectedMovieId(Guid.NewGuid());
    }

    public static SelectedMovieId Create(Guid value)
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