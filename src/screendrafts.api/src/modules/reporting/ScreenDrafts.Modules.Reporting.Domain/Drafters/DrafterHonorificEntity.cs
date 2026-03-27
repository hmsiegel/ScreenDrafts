namespace ScreenDrafts.Modules.Reporting.Domain.Drafters;

public sealed class DrafterHonorificEntity : Entity<DrafterHonorificId>
{
  private DrafterHonorificEntity(
    Guid drafterIdValue,
    DrafterHonorific honorific,
    int appearanceCount,
    DateTime updateAtUtc,
    DrafterHonorificId? id = null)
    : base(id ?? DrafterHonorificId.CreateUnique())
  {
    DrafterIdValue = drafterIdValue;
    Honorific = honorific;
    AppearanceCount = appearanceCount;
    UpdateAtUtc = updateAtUtc;
  }

  private DrafterHonorificEntity()
  {

  }

  public Guid DrafterIdValue { get; private set; }
  public DrafterHonorific Honorific { get; private set; } = DrafterHonorific.None;
  public int AppearanceCount { get; private set; }
  public DateTime UpdateAtUtc { get; private set; }

  public static DrafterHonorificEntity Create(
    Guid drafterIdValue,
    DrafterHonorific honorific,
    int appearanceCount)
  {
    return new DrafterHonorificEntity(
      drafterIdValue: drafterIdValue,
      honorific: honorific,
      appearanceCount: appearanceCount,
      updateAtUtc: DateTime.UtcNow);
  }
}
