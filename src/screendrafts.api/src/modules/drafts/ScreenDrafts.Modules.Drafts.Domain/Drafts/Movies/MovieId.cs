namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Movies;

public sealed record MovieId(Ulid Value)
{
  public Ulid Value { get; private set; } = Value;

  public static MovieId CreateUnique() => new(Ulid.NewUlid());
  public static MovieId FromString(string value) => new(Ulid.Parse(value, CultureInfo.InvariantCulture));
  public static MovieId Create(Ulid value) => new(value);
}
