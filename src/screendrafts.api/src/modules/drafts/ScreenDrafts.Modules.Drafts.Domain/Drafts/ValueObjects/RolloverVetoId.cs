namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record RolloverVetoId(Ulid Value)
{
  public Ulid Value { get; } = Value;

  public static RolloverVetoId CreateUnique() => new(Ulid.NewUlid());

  public static RolloverVetoId FromString(string value) => new(Ulid.Parse(value, CultureInfo.InvariantCulture));

  public static RolloverVetoId Create(Ulid value) => new(value);
}
