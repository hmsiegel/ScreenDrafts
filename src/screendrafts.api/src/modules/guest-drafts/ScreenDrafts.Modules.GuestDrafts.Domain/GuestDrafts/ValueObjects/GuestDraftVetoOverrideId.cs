namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.ValueObjects;

public sealed record GuestDraftVetoOverrideId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static GuestDraftVetoOverrideId CreateUnique() => new(Guid.NewGuid());

  public static GuestDraftVetoOverrideId FromString(string value) => new(Guid.Parse(value));

  public static GuestDraftVetoOverrideId Create(Guid value) => new(value);
}
