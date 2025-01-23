namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record MovieId(Guid Value)
{
  public Guid Value { get; private set; } = Value;

  public static MovieId CreateUnique() => new(Guid.NewGuid());
  public static MovieId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
  public static MovieId Create(Guid value) => new(value);
}
