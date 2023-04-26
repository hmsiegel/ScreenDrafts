namespace ScreenDrafts.Domain.HostAggregate.ValueObject;
public sealed class HostId : AggregateRootId<DefaultIdType>
{
    public override DefaultIdType Value { get; protected set; }

    private HostId(DefaultIdType value) => Value = value;

    public static HostId CreateUnique() => new(DefaultIdType.NewGuid());
    public static HostId Create(DefaultIdType value) => new(value);

    public override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    private HostId()
    {
    }
}
