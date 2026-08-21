namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftPickResponse
{
  public int PlayOrder { get; init; }
  public int Position { get; init; }
  public required string MoviePublicId { get; init; }
  public required string MovieTitle { get; init; }
  public string? MovieVersionName { get; init; }
  public string? ActedByPublicId { get; init; }
  public Guid PlayedByParticipantIdValue { get; init; }
  public ParticipantKind PlayedByParticipantKindValue { get; init; } = default!;

  /// <summary>
  /// This pick's full veto history, ordered by Sequence. Normally holds at most one
  /// entry — more than one only occurs when a veto was overridden and the resulting
  /// override was itself overridden, re-vetoing the pick (see Pick.Vetoes / Pick.CurrentVeto
  /// on the domain side). Consumers that only care about the pick's current state want the
  /// last entry, not the first.
  /// </summary>
  public Collection<GetDraftVetoResponse> Vetoes { get; init; } = [];
  public GetDraftCommissionerOverrideResponse? CommissionerOverride { get; init; }

  /// <summary>
  /// Null for standard drafts. For SpeedDraft, this is the 1-based
  /// index of the sub-draft within the part that this pick belongs to.
  /// </summary>
  public int? SubDraftIndex { get; init; }
}
