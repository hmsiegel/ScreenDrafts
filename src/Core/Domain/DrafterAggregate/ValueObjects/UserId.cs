namespace ScreenDrafts.Domain.DrafterAggregate.ValueObjects;
public sealed class UserId : ValueObject
{
    public string? Value { get; private set; }

    private UserId(string? value)
    {
        Value = value;
    }

    public static UserId Create(string? value)
    {
        return new UserId(value);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value!;
    }

#pragma warning disable CS8618
    private UserId()
    {
    }
#pragma warning restore CS8618
}
