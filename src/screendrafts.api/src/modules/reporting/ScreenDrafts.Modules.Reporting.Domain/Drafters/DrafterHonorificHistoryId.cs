namespace ScreenDrafts.Modules.Reporting.Domain.Drafters;

public sealed record DrafterHonorificHistoryId(Guid Value)
{
  public static DrafterHonorificHistoryId CreateUnique() => new(Guid.NewGuid());
  public static DrafterHonorificHistoryId Create(Guid value) => new(value);
}
