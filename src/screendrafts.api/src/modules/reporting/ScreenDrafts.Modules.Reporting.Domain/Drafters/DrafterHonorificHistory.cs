namespace ScreenDrafts.Modules.Reporting.Domain.Drafters;

public sealed class DrafterHonorificHistory : Entity<DrafterHonorificHistoryId>
{
  private DrafterHonorificHistory(
    Guid drafterIdValue,
    DrafterHonorific honorific,
    int appearanceCount,
    DateTime achievedAt,
    DrafterHonorificHistoryId? id = null)
    : base(id ?? DrafterHonorificHistoryId.CreateUnique())
  {
    DrafterIdValue = drafterIdValue;
    Honorific = honorific;
    AppearanceCount = appearanceCount;
    AchievedAt = achievedAt;
  }

  private DrafterHonorificHistory() { }

  public Guid DrafterIdValue { get; private set; }
  public DrafterHonorific Honorific { get; private set; } = DrafterHonorific.None;
  public int AppearanceCount { get; private set; }
  public DateTime AchievedAt { get; private set; }

  public static DrafterHonorificHistory Create(
    Guid drafterIdValue,
    DrafterHonorific honorific,
    int appearanceCount)
  {
    return new DrafterHonorificHistory(
      drafterIdValue: drafterIdValue,
      honorific: honorific,
      appearanceCount: appearanceCount,
      achievedAt: DateTime.UtcNow);
  }
}
