namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.UpdateGuestDraft;

internal sealed record UpdateGuestDraftPositionInput
{
  public required string Name { get; init; }
  public required IReadOnlyList<int> Picks { get; init; }
  public bool HasBonusVeto { get; init; }
  public bool HasBonusVetoOverride { get; init; }
  public bool HasBonusFungibleToken { get; init; }
}
