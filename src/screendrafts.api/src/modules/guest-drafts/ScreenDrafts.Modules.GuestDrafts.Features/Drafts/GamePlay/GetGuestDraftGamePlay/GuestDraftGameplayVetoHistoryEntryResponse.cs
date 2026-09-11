namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.GamePlay.GetGuestDraftGamePlay;

internal sealed record GuestDraftGameplayVetoHistoryEntryResponse
{
  public int Sequence { get; init; }
  public string VetoedByDisplayName { get; init; } = default!;
  public bool WasVetoFungible { get; init; }
  public bool IsOverridden { get; init; }
  public string? OverriddenByDisplayName { get; init; }
  public bool WasOverrideFungible { get; init; }
}
