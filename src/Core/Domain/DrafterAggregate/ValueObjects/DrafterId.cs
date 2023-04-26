namespace ScreenDrafts.Domain.DrafterAggregate.ValueObjects;
public sealed class DrafterId : AggregateRootId<DefaultIdType>
{
    public override DefaultIdType Value { get; protected set; }

    private DrafterId(DefaultIdType value) => Value = value;

    public static DrafterId Create(DefaultIdType value) => new(value);
    public static DrafterId CreateUnique() => new(DefaultIdType.NewGuid());

    public override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    private DrafterId()
    {
    }
}
