namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.GamePlay.GetGuestDraftGamePlay;

internal sealed record GameplayPositionResponse
{
  public string PositionPublicId { get; init; } = default!;
  public string Name { get; init; } = default!;
  public int[] Picks { get; init; } = [];
  public bool HasBonusVeto { get; init; }
  public bool HasBonusVetoOverride { get; init; }
  public bool HasBonusFungibleToken { get; init; }
  public string? AssignedParticipantPublicId { get; init; }
  public string? AssignedParticipantDisplayName { get; init; }
}
