namespace ScreenDrafts.Domain.HostAggregate.ValueObject;
public sealed class HostId : AggregateRootId<DefaultIdType>
{
    public override DefaultIdType Value { get; protected set; }

    private HostId(DefaultIdType value) => Value = value;

    public static HostId Create() => new(DefaultIdType.NewGuid());

    public override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    private HostId()
    {
    }
}
