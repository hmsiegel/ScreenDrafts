namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.ValueObjects;

public sealed record VetoOverrideId(Guid Value)
{
  public Guid Value { get; } = Value;

  public static VetoOverrideId CreateUnique() => new(Guid.NewGuid());

  public static VetoOverrideId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static VetoOverrideId Create(Guid value) => new(value);
}
