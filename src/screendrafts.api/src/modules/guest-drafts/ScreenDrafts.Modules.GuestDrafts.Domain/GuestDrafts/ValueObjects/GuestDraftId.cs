using System.Globalization;
using ScreenDrafts.Common.Domain;

namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.ValueObjects;

public sealed record GuestDraftId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static GuestDraftId CreateUnique() => new(Guid.NewGuid());

  public static GuestDraftId Create(Guid value) => new(value);

  public static GuestDraftId FromString(string value) =>
    new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static GuestDraftId Empty => new(Guid.Empty);
}
