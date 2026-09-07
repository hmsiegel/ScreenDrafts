namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.GamePlay.GetGuestDraftGamePlay;

/// <summary>
/// One drafted slot. PlayedByParticipantId and RevealAuthorizedParticipantId
/// are raw GuestDraftParticipant ids -- the same ids the domain events and
/// SignalR broadcasts carry, so the frontend matches a pick to "is this mine
/// to reveal" with a Guid comparison against CallerContextResponse.ParticipantId.
/// </summary>
internal sealed record GameplayPickResponse
{
  public int PlayOrder { get; init; }
  public int Position { get; init; }
  public string? MoviePublicId { get; init; }
  public string? MovieTitle { get; init; }
  public Guid PlayedByParticipantId { get; init; }
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
  public int VetoSequence { get; init; }
  public Guid? RevealAuthorizedParticipantId { get; init; }
  public string? RevealAuthorizedByDisplayName { get; init; }
  public IReadOnlyList<GameplayVetoHistoryEntryResponse> VetoHistory { get; init; } = [];
}
