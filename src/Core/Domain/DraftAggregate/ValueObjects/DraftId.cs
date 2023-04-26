using ErrorOr;

using ScreenDrafts.Domain.Common.Errors;

namespace ScreenDrafts.Domain.DraftAggregate.ValueObjects;
public sealed class DraftId : AggregateRootId<DefaultIdType>
{
    public override DefaultIdType Value { get; protected set; }

    private DraftId(DefaultIdType value) => Value = value;

    public static DraftId CreateUnique() => new(DefaultIdType.NewGuid());
    public static DraftId Create(DefaultIdType value) => new(value);
    public static ErrorOr<DraftId> Create(string? value)
    {
        return !Guid.TryParse(value, out var id)
            ? (ErrorOr<DraftId>)Errors.Draft.InvalidDraftId
            : (ErrorOr<DraftId>)new DraftId(id);
    }

    public override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    private DraftId()
    {
    }
}
