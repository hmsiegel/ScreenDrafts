namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafters;

public sealed record GuestDrafterId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static GuestDrafterId CreateUnique() => new(Guid.NewGuid());

  public static GuestDrafterId FromString(string value) =>
    new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static GuestDrafterId Create(Guid value) => new(value);

  public static GuestDrafterId Empty => new(Guid.Empty);
}
