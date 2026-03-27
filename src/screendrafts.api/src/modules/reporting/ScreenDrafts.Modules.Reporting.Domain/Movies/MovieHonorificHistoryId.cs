namespace ScreenDrafts.Modules.Reporting.Domain.Movies;

public sealed record MovieHonorificHistoryId(Guid Value)
{
  public static MovieHonorificHistoryId CreateUnique() => new(Guid.NewGuid());
  public static MovieHonorificHistoryId Create(Guid value) => new(value);
}
