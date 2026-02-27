namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetDraftPosition;

internal sealed record DraftPositionRequest
{
  public required string Name { get; init; }
  public IReadOnlyList<int> Picks { get; init; } = [];
  public bool HasBonusVeto { get; init; }
  public bool HasBonusVetoOverride { get; init; }
}
