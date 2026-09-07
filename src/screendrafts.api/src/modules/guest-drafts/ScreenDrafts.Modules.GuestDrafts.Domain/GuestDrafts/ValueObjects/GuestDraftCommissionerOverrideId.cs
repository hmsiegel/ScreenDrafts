namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.ValueObjects;

public sealed record GuestDraftCommissionerOverrideId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static GuestDraftCommissionerOverrideId CreateUnique() => new(Guid.NewGuid());

  public static GuestDraftCommissionerOverrideId FromString(string value) => new(Guid.Parse(value));

  public static GuestDraftCommissionerOverrideId Create(Guid value) => new(value);
}
