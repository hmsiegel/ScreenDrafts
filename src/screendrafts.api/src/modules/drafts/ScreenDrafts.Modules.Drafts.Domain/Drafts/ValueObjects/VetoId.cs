namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record VetoId(Ulid Value)
{
  public Ulid Value { get; } = Value;

  public static VetoId CreateUnique() => new(Ulid.NewUlid());

  public static VetoId FromString(string value) => new(Ulid.Parse(value, CultureInfo.InvariantCulture));

  public static VetoId Create(Ulid value) => new(value);
}
