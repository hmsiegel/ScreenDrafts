namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.GamePlay.GetGuestDraftGamePlay;

internal sealed record GameplayParticipantResponse
{
  public string ParticipantPublicId { get; init; } = default!;
  public bool IsOwner { get; init; }
  public string DisplayName { get; init; } = default!;
  public int VetoTokensRemaining { get; init; }
  public int OverrideTokensRemaining { get; init; }
  public int FungibleTokensRemaining { get; init; }
  public int CommissionerOverridesUsed { get; init; }
}
