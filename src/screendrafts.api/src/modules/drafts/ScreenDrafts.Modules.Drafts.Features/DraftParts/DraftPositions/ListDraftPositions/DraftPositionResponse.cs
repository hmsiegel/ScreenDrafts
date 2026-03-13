namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ListDraftPositions;

public sealed record DraftPositionResponse
{
  public string PublicId { get; init; } = default!;
  public string Name { get; init; } = default!;
  public IReadOnlyCollection<int> Picks { get; init; } = [];
  public bool HasBonusVeto { get; init; }
  public bool HasBonusVetoOverride { get; init; }
  public DraftPositionAssignmentResponse? AssignedTo { get; init; }
}
