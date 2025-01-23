namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record VetoId(Guid Value)
{
  public Guid Value { get; } = Value;

  public static VetoId CreateUnique() => new(Guid.NewGuid());

  public static VetoId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static VetoId Create(Guid value) => new(value);
}
