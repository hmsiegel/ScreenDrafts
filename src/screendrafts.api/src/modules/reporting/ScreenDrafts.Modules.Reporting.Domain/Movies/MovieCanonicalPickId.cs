namespace ScreenDrafts.Modules.Reporting.Domain.Movies;

public sealed record MovieCanonicalPickId(Guid Value)
{
  public static MovieCanonicalPickId CreateUnique() => new(Guid.NewGuid());
  public static MovieCanonicalPickId Create(Guid value) => new(value);
}
