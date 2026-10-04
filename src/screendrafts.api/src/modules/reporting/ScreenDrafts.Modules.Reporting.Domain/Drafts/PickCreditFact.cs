namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

/// <summary>
/// One row per drafter credited for a pick. Solo picks credit the player; team picks credit
/// each member snapshotted at pick time. Per-drafter metrics join through this table, so team
/// picks never double-count in draft-level or title-level metrics, which read pick_facts only.
/// Rows are written with Dapper by RecordPartStatsCommandHandler. Create exists for tests and seeding.
/// </summary>
public sealed class PickCreditFact : Entity
{
  private PickCreditFact(
    Guid id,
    Guid pickId,
    Guid draftId,
    string draftPartPublicId,
    Guid drafterIdValue,
    string drafterPublicId,
    string drafterName,
    DateTime recordedAtUtc
  )
  {
    Id = id;
    PickId = pickId;
    DraftId = draftId;
    DraftPartPublicId = draftPartPublicId;
    DrafterIdValue = drafterIdValue;
    DrafterPublicId = drafterPublicId;
    DrafterName = drafterName;
    RecordedAtUtc = recordedAtUtc;
  }

  private PickCreditFact() { }

  public Guid PickId { get; private set; }
  public Guid DraftId { get; private set; }
  public string DraftPartPublicId { get; private set; } = default!;
  public Guid DrafterIdValue { get; private set; }
  public string DrafterPublicId { get; private set; } = default!;
  public string DrafterName { get; private set; } = default!;
  public DateTime RecordedAtUtc { get; private set; }

  public static PickCreditFact Create(
    Guid id,
    Guid pickId,
    Guid draftId,
    string draftPartPublicId,
    Guid drafterIdValue,
    string drafterPublicId,
    string drafterName,
    DateTime recordedAtUtc
  ) =>
    new(
      id,
      pickId,
      draftId,
      draftPartPublicId,
      drafterIdValue,
      drafterPublicId,
      drafterName,
      recordedAtUtc
    );
}
