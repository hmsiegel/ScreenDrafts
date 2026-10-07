namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

/// <summary>
/// One row per pick played in a completed draft part (vetoed and commissioner-overridden
/// picks included). Id is the pick's id in the Drafts module. Rows are written with Dapper by
/// RecordPartStatsCommandHandler; EF owns the table schema. Create exists for tests and seeding.
/// Canonical-ness is NOT stored: canonical_policy 2 depends on a main-feed release that can
/// arrive after completion, so queries resolve it against draft_part_releases at read time.
/// </summary>
public sealed class PickFact : Entity
{
  private PickFact(
    Guid id,
    Guid draftId,
    string draftPublicId,
    string draftPartPublicId,
    int partIndex,
    string draftTitle,
    string draftType,
    string seriesName,
    int canonicalPolicy,
    int? subDraftIndex,
    int position,
    int playOrder,
    string mediaPublicId,
    string mediaTitle,
    int playedByKind,
    Guid playedByIdValue,
    string? playedByPublicId,
    string playedByName,
    int vetoCount,
    bool wasVetoed,
    bool wasVetoOverridden,
    bool wasCommissionerOverridden,
    DateTime recordedAtUtc
  )
  {
    Id = id;
    DraftId = draftId;
    DraftPublicId = draftPublicId;
    DraftPartPublicId = draftPartPublicId;
    PartIndex = partIndex;
    DraftTitle = draftTitle;
    DraftType = draftType;
    SeriesName = seriesName;
    CanonicalPolicy = canonicalPolicy;
    SubDraftIndex = subDraftIndex;
    Position = position;
    PlayOrder = playOrder;
    MediaPublicId = mediaPublicId;
    MediaTitle = mediaTitle;
    PlayedByKind = playedByKind;
    PlayedByIdValue = playedByIdValue;
    PlayedByPublicId = playedByPublicId;
    PlayedByName = playedByName;
    VetoCount = vetoCount;
    WasVetoed = wasVetoed;
    WasVetoOverridden = wasVetoOverridden;
    WasCommissionerOverridden = wasCommissionerOverridden;
    RecordedAtUtc = recordedAtUtc;
  }

  private PickFact() { }

  public Guid DraftId { get; private set; }
  public string DraftPublicId { get; private set; } = default!;
  public string DraftPartPublicId { get; private set; } = default!;
  public int PartIndex { get; private set; }
  public string DraftTitle { get; private set; } = default!;
  public string DraftType { get; private set; } = default!;
  public string SeriesName { get; private set; } = default!;
  public int CanonicalPolicy { get; private set; }
  public int? SubDraftIndex { get; private set; }
  public int Position { get; private set; }
  public int PlayOrder { get; private set; }
  public string MediaPublicId { get; private set; } = default!;
  public string MediaTitle { get; private set; } = default!;

  /// <summary>0 = drafter, 1 = team, 2 = community.</summary>
  public int PlayedByKind { get; private set; }
  public Guid PlayedByIdValue { get; private set; }
  public string? PlayedByPublicId { get; private set; }
  public string PlayedByName { get; private set; } = default!;

  public int VetoCount { get; private set; }
  public bool WasVetoed { get; private set; }

  /// <summary>True when the pick's last veto (highest sequence) was overridden.</summary>
  public bool WasVetoOverridden { get; private set; }
  public bool WasCommissionerOverridden { get; private set; }
  public DateTime RecordedAtUtc { get; private set; }

  public static PickFact Create(
    Guid id,
    Guid draftId,
    string draftPublicId,
    string draftPartPublicId,
    int partIndex,
    string draftTitle,
    string draftType,
    string seriesName,
    int canonicalPolicy,
    int? subDraftIndex,
    int position,
    int playOrder,
    string mediaPublicId,
    string mediaTitle,
    int playedByKind,
    Guid playedByIdValue,
    string? playedByPublicId,
    string playedByName,
    int vetoCount,
    bool wasVetoed,
    bool wasVetoOverridden,
    bool wasCommissionerOverridden,
    DateTime recordedAtUtc
  ) =>
    new(
      id,
      draftId,
      draftPublicId,
      draftPartPublicId,
      partIndex,
      draftTitle,
      draftType,
      seriesName,
      canonicalPolicy,
      subDraftIndex,
      position,
      playOrder,
      mediaPublicId,
      mediaTitle,
      playedByKind,
      playedByIdValue,
      playedByPublicId,
      playedByName,
      vetoCount,
      wasVetoed,
      wasVetoOverridden,
      wasCommissionerOverridden,
      recordedAtUtc
    );
}
