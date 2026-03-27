namespace ScreenDrafts.Modules.Reporting.Domain.Drafters;

public sealed record DrafterHonorificId(Guid Value)
{
  public static DrafterHonorificId Create(Guid value) => new(value);
  public static DrafterHonorificId CreateUnique() => new(Guid.NewGuid());
}
