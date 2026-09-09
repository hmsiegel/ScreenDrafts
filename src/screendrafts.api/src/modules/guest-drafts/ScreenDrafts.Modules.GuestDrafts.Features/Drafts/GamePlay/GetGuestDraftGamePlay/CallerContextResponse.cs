namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.GamePlay.GetGuestDraftGamePlay;

/// <summary>
/// The caller's own standing in this draft. ParticipantId is the raw
/// GuestDraftParticipant id -- the same Guid threaded through every pick's
/// PlayedByParticipantId/RevealAuthorizedParticipantId below, and the same
/// Guid DraftHub groups by (GuestDraftParticipantGroupName). The frontend
/// finds its own participantId for JoinGuestDraftAsync here, and tells
/// whether it must reveal a given pick with a plain Guid comparison against
/// GameplayPickResponse.RevealAuthorizedParticipantId -- no PublicId
/// round-trip required.
/// </summary>
internal sealed record CallerContextResponse
{
  public bool IsOwner { get; init; }
  public bool IsParticipant { get; init; }
  public Guid? ParticipantId { get; init; }
  public string? ParticipantPublicId { get; init; }
}
