namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.ValueObjects;

public sealed record RolloverVetoOverrideId(Guid Value)
{
  public Guid Value { get; } = Value;

  public static RolloverVetoOverrideId CreateUnique() => new(Guid.NewGuid());

  public static RolloverVetoOverrideId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static RolloverVetoOverrideId Create(Guid value) => new(value);
}
