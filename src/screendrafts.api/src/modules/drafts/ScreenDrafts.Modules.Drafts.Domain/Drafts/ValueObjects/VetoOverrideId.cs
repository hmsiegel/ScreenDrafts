namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record VetoOverrideId(Ulid Value)
{
  public Ulid Value { get; } = Value;

  public static VetoOverrideId CreateUnique() => new(Ulid.NewUlid());

  public static VetoOverrideId FromString(string value) => new(Ulid.Parse(value, CultureInfo.InvariantCulture));

  public static VetoOverrideId Create(Ulid value) => new(value);
}
