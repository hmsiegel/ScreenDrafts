namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed record CreateDraftPositionRequest
{
  public required string Name { get; init; }
  public required IReadOnlyList<int> Picks { get; init; }
  public bool HasBonusVeto { get; init; }
  public bool HasBonusVetoOverride { get; init; }
}
