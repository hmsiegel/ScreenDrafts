namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

/// <summary>
/// One row per veto. Id is the veto's id in the Drafts module; PickId points at pick_facts.
/// IdValue columns hold the drafter id when the kind is 0 and the team id when the kind is 1.
/// Rows are written with Dapper by RecordPartStatsCommandHandler. Create exists for tests and seeding.
/// </summary>
public sealed class VetoFact : Entity
{
  private VetoFact(
    Guid id,
    Guid pickId,
    Guid draftId,
    string draftPartPublicId,
    int sequence,
    int issuedByKind,
    Guid issuedByIdValue,
    string? issuedByPublicId,
    string issuedByName,
    bool isOverridden,
    int? overriddenByKind,
    Guid? overriddenByIdValue,
    string? overriddenByPublicId,
    string? overriddenByName,
    bool isSelfVeto,
    DateTime recordedAtUtc
  )
  {
    Id = id;
    PickId = pickId;
    DraftId = draftId;
    DraftPartPublicId = draftPartPublicId;
    Sequence = sequence;
    IssuedByKind = issuedByKind;
    IssuedByIdValue = issuedByIdValue;
    IssuedByPublicId = issuedByPublicId;
    IssuedByName = issuedByName;
    IsOverridden = isOverridden;
    OverriddenByKind = overriddenByKind;
    OverriddenByIdValue = overriddenByIdValue;
    OverriddenByPublicId = overriddenByPublicId;
    OverriddenByName = overriddenByName;
    IsSelfVeto = isSelfVeto;
    RecordedAtUtc = recordedAtUtc;
  }

  private VetoFact() { }

  public Guid PickId { get; private set; }
  public Guid DraftId { get; private set; }
  public string DraftPartPublicId { get; private set; } = default!;
  public int Sequence { get; private set; }

  public int IssuedByKind { get; private set; }
  public Guid IssuedByIdValue { get; private set; }
  public string? IssuedByPublicId { get; private set; }
  public string IssuedByName { get; private set; } = default!;

  public bool IsOverridden { get; private set; }
  public int? OverriddenByKind { get; private set; }
  public Guid? OverriddenByIdValue { get; private set; }
  public string? OverriddenByPublicId { get; private set; }
  public string? OverriddenByName { get; private set; }

  /// <summary>The issuer is the participant who played the vetoed pick.</summary>
  public bool IsSelfVeto { get; private set; }
  public DateTime RecordedAtUtc { get; private set; }

  public static VetoFact Create(
    Guid id,
    Guid pickId,
    Guid draftId,
    string draftPartPublicId,
    int sequence,
    int issuedByKind,
    Guid issuedByIdValue,
    string? issuedByPublicId,
    string issuedByName,
    bool isOverridden,
    int? overriddenByKind,
    Guid? overriddenByIdValue,
    string? overriddenByPublicId,
    string? overriddenByName,
    bool isSelfVeto,
    DateTime recordedAtUtc
  ) =>
    new(
      id,
      pickId,
      draftId,
      draftPartPublicId,
      sequence,
      issuedByKind,
      issuedByIdValue,
      issuedByPublicId,
      issuedByName,
      isOverridden,
      overriddenByKind,
      overriddenByIdValue,
      overriddenByPublicId,
      overriddenByName,
      isSelfVeto,
      recordedAtUtc
    );
}
