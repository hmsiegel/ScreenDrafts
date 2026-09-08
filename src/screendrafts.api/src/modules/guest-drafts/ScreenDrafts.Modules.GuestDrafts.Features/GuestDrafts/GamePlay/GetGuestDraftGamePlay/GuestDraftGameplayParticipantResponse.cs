namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.GamePlay.GetGuestDraftGamePlay;

/// <summary>
/// One seat in the draft. ParticipantId is the raw GuestDraftParticipant id.
/// This is the only place in the response that also exposes the underlying
/// GuestDrafter's own PublicId -- every pick/position reference elsewhere
/// carries ParticipantId alone, matching GetDraftPartGameplayResponse's shape.
/// </summary>
internal sealed record GuestDraftGameplayParticipantResponse
{
  public Guid ParticipantId { get; init; }
  public string ParticipantPublicId { get; init; } = default!;
  public bool IsOwner { get; init; }
  public string DisplayName { get; init; } = default!;
  public int VetoTokensRemaining { get; init; }
  public int OverrideTokensRemaining { get; init; }
  public int FungibleTokensRemaining { get; init; }
  public int CommissionerOverridesUsed { get; init; }
}
