namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

internal sealed record GameplayPickResponse
{
  public int PlayOrder { get; init; }
  public int BoardPosition { get; init; }
  public string MovieTitle { get; init; } = default!;
  public string? MovieYear { get; init; }
  public int? TmdbId { get; init; }
  public string? ImdbId { get; init; }
  public int? IgdbId { get; init; }
  public int? MediaType { get; init; }
  public Guid PlayedById { get; init; }
  public int PlayedByKind { get; init; }
  public string PlayedByName { get; init; } = default!;
  public bool WasVetoed { get; init; }
  public bool WasVetoOverridden { get; init; }
  public bool WasCommissionerOverride { get; init; }
  public string? VetoedByName { get; init; }
  public string? SavedByName { get; init; }
  public IReadOnlyList<GameplayVetoHistoryEntryResponse> VetoHistory { get; init; } = [];

  /// <summary>
  /// True when the pick's current veto (Pick.CurrentVeto) was paid for by a fungible
  /// token rather than a normal veto. Meaningless when WasVetoed is false.
  /// </summary>
  public bool WasVetoFungible { get; init; }

  /// <summary>
  /// True when the override that saved this pick was paid for by a fungible token
  /// rather than a normal, awarded override. Meaningless when WasVetoOverridden is false.
  /// </summary>
  public bool WasVetoOverrideFungible { get; init; }

  /// <summary>
  /// 1-based position of the pick's current veto within its full veto history — mirrors
  /// Veto.Sequence. Normally 1. A value of 2 means this pick was vetoed, that veto was
  /// overridden, and the override was itself overridden (re-vetoing the pick) — the wizard
  /// should offer "Veto" again on a WasVetoOverridden pick when it's still the most recent
  /// play, the same as it does for a fresh "landed" pick.
  /// </summary>
  public int VetoSequence { get; init; }

  /// <summary>
  /// Only set on a hostless draft (GetDraftPartGameplayResponse.IsHostless) — the
  /// participant this pick was "sent to," who is therefore the one authorized to reveal
  /// it. Null for every pick on a hosted draft, where reveal authority belongs to the
  /// primary host instead. See Pick.RevealAuthorizedParticipant's remarks.
  /// </summary>
  public Guid? RevealAuthorizedParticipantId { get; init; }

  public string? RevealAuthorizedByName { get; init; }
}
