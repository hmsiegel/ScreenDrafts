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
  public GetDraftVetoResponse? Veto { get; init; }
  public GetDraftCommissionerOverrideResponse? CommissionerOverride { get; init; }

  /// <summary>
  /// Null for standard drafts. For SpeedDraft, this is the 1-based
  /// index of the sub-draft within the part that this pick belongs to.
  /// </summary>
  public int? SubDraftIndex { get; init; }
}
