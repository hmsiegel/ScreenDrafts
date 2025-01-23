namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record RolloverVetoId(Guid Value)
{
  public Guid Value { get; } = Value;

  public static RolloverVetoId CreateUnique() => new(Guid.NewGuid());

  public static RolloverVetoId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static RolloverVetoId Create(Guid value) => new(value);
}
