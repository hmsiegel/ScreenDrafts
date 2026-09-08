namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

public sealed record CommissionerOverrideId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static CommissionerOverrideId CreateUnique() => new(Guid.NewGuid());

  public static CommissionerOverrideId FromString(string value) => new(Guid.Parse(value));

  public static CommissionerOverrideId Create(Guid value) => new(value);
}
