namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetCustomPositions;

internal sealed record PositionInput
{
  public required string Name { get; init; }
  public required IReadOnlyList<int> Picks { get; init; }
  public bool HasBonusVeto { get; init; }
  public bool HasBonusVetoOverride { get; init; }
  public bool HasBonusFungibleToken { get; init; }
}
