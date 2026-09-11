namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

public sealed record VetoOverrideId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static VetoOverrideId CreateUnique() => new(Guid.NewGuid());

  public static VetoOverrideId FromString(string value) => new(Guid.Parse(value));

  public static VetoOverrideId Create(Guid value) => new(value);
}
