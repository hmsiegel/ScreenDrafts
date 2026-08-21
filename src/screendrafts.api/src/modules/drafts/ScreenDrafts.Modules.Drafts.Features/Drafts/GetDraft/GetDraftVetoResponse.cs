namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftVetoResponse
{
  public Guid IssuedByParticipantId { get; init; }
  public string? IssuedByDisplayName { get; init; }
  public string? ActedByPublicId { get; init; }
  public string? ActedByDisplayName { get; init; }
  public bool IsOverridden { get; init; }
  public string? Note { get; init; }
  public DateTime OccurredOnUtc { get; init; }
  public GetDraftVetoOverrideResponse? Override { get; init; }

  /// <summary>
  /// 1-based position of this veto within its pick's veto history — see
  /// GetDraftPickResponse.Vetoes. Normally 1; a second entry (Sequence 2) only occurs
  /// when a first veto was overridden and the override was itself overridden, re-vetoing
  /// the pick.
  /// </summary>
  public int Sequence { get; init; }

  /// <summary>
  /// True when this veto was paid for by a fungible token (BUV, Rabbit's Foot, etc.)
  /// rather than a normal veto.
  /// </summary>
  public bool SpentFromFungiblePool { get; init; }
}
