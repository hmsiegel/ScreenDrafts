namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record RolloverVetoOverrideId(Ulid Value)
{
  public Ulid Value { get; } = Value;

  public static RolloverVetoOverrideId CreateUnique() => new(Ulid.NewUlid());

  public static RolloverVetoOverrideId FromString(string value) => new(Ulid.Parse(value, CultureInfo.InvariantCulture));

  public static RolloverVetoOverrideId Create(Ulid value) => new(value);
}
