namespace ScreenDrafts.Domain.DraftAggregate.ValueObjects;
public sealed class DraftId : AggregateRootId<DefaultIdType>
{
    public override DefaultIdType Value { get; protected set; }

    private DraftId(DefaultIdType value) => Value = value;

    public static DraftId Create() => new(DefaultIdType.NewGuid());

    public override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    private DraftId()
    {
    }
}
