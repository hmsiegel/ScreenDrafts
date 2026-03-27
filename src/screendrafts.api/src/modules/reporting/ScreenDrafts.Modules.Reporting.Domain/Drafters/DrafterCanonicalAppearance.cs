namespace ScreenDrafts.Modules.Reporting.Domain.Drafters;

public sealed class DrafterCanonicalAppearance : Entity<DrafterCanonicalAppearanceId>
{
  private DrafterCanonicalAppearance(
    Guid drafterIdValue,
    string draftPartPublicId,
    bool hasMainFeedRelease,
    DateTime appearedAt,
    DrafterCanonicalAppearanceId? id = null)
    : base(id ?? DrafterCanonicalAppearanceId.CreateUnique())
  {
    DrafterIdValue = drafterIdValue;
    DraftPartPublicId = draftPartPublicId;
    HasMainFeedRelease = hasMainFeedRelease;
    AppearedAt = appearedAt;
  }

  private DrafterCanonicalAppearance() { }

  public Guid DrafterIdValue { get; private set; }
  public string DraftPartPublicId { get; private set; } = default!;
  public bool HasMainFeedRelease { get; private set; }
  public DateTime AppearedAt { get; private set; }

  public static DrafterCanonicalAppearance Create(
    Guid drafterIdValue,
    string draftPartPublicId,
    bool hasMainFeedRelease)
  {
    return new DrafterCanonicalAppearance(
      drafterIdValue: drafterIdValue,
      draftPartPublicId: draftPartPublicId,
      hasMainFeedRelease: hasMainFeedRelease,
      appearedAt: DateTime.UtcNow);
  }
}
