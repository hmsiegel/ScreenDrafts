namespace ScreenDrafts.Modules.Reporting.Domain.Movies;

public sealed record MovieHonorificId(Guid Value)
{
  public static MovieHonorificId CreateUnique() => new(Guid.NewGuid());
  public static MovieHonorificId Create(Guid value) => new(value);
}
