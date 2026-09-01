namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.ValueObjects;

public sealed record GuestDraftPickId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static GuestDraftPickId CreateUnique() => new(Guid.NewGuid());

  public static GuestDraftPickId Create(Guid value) => new(value);

  public static GuestDraftPickId FromString(string value) =>
    new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static GuestDraftPickId Empty => new(Guid.Empty);
}
