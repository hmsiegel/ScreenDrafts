namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

public sealed record VetoId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static VetoId CreateUnique() => new(Guid.NewGuid());

  public static VetoId FromString(string value) => new(Guid.Parse(value));

  public static VetoId Create(Guid value) => new(value);
}
