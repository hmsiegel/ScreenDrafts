namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.GamePlay.GetGuestDraftGamePlay;

internal sealed record GameplayPickResponse
{
  public int PlayOrder { get; init; }
  public int Position { get; init; }

  /// <summary>
  /// Null on an unrevealed pick unless the caller is the picker, the designated
  /// revealer, or the owner -- concealed the same way canonical hides a pending
  /// sub-draft's subject from non-hosts. "Who picked" is never secret, only "what."
  /// </summary>
  public string? MoviePublicId { get; init; }
  public string? MovieTitle { get; init; }

  public string PlayedByParticipantPublicId { get; init; } = default!;
  public string PlayedByDisplayName { get; init; } = default!;
  public bool IsRevealed { get; init; }
  public bool WasVetoed { get; init; }
  public bool WasVetoOverridden { get; init; }
  public bool WasCommissionerOverride { get; init; }
  public bool IsActiveOnFinalBoard { get; init; }
  public bool IsEligibleForRePick { get; init; }
  public string? VetoedByDisplayName { get; init; }
  public string? SavedByDisplayName { get; init; }
  public bool WasVetoFungible { get; init; }
  public bool WasVetoOverrideFungible { get; init; }

  /// <summary>1-based position of the pick's current veto within its full veto
  /// history -- mirrors GuestDraftVeto.Sequence. See GameplayVetoHistoryEntryResponse
  /// for the full history, not just the current entry.</summary>
  public int VetoSequence { get; init; }

  public string? RevealAuthorizedParticipantPublicId { get; init; }
  public string? RevealAuthorizedByDisplayName { get; init; }

  public IReadOnlyList<GameplayVetoHistoryEntryResponse> VetoHistory { get; init; } = [];
}
