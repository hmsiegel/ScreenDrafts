namespace ScreenDrafts.Modules.Reporting.Domain.Drafters;

public sealed class DrafterCanonicalAppearance : Entity<DrafterCanonicalAppearanceId>
{
  private DrafterCanonicalAppearance(
    Guid drafterIdValue,
    Guid draftId,
    string draftPartPublicId,
    bool hasMainFeedRelease,
    DateTime appearedAt,
    DrafterCanonicalAppearanceId? id = null
  )
    : base(id ?? DrafterCanonicalAppearanceId.CreateUnique())
  {
    DrafterIdValue = drafterIdValue;
    DraftId = draftId;
    DraftPartPublicId = draftPartPublicId;
    HasMainFeedRelease = hasMainFeedRelease;
    AppearedAt = appearedAt;
  }

  private DrafterCanonicalAppearance() { }

  public Guid DrafterIdValue { get; private set; }

  /// <summary>
  /// The draft this part belongs to. Honorifics count distinct drafts, so a drafter who appears
  /// in several parts of one draft counts once. Guid.Empty marks a row not yet linked to a draft.
  /// </summary>
  public Guid DraftId { get; private set; }
  public string DraftPartPublicId { get; private set; } = default!;
  public bool HasMainFeedRelease { get; private set; }
  public DateTime AppearedAt { get; private set; }

  public static DrafterCanonicalAppearance Create(
    Guid drafterIdValue,
    Guid draftId,
    string draftPartPublicId,
    bool hasMainFeedRelease
  )
  {
    return new DrafterCanonicalAppearance(
      drafterIdValue: drafterIdValue,
      draftId: draftId,
      draftPartPublicId: draftPartPublicId,
      hasMainFeedRelease: hasMainFeedRelease,
      appearedAt: DateTime.UtcNow
    );
  }
}
