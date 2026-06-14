namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

internal sealed record GameplayDraftPositionResponse
{
  public string PositionPublicId { get; init; } = default!;
  public string PositionName { get; init; } = default!;
  public int[] OwnedBoardSlots { get; init; } = [];
  public bool HasBonusVeto { get; init; }
  public bool HasBonusVetoOverride { get; init; }
  public Guid? AssignedParticipantId { get; init; }
  public int? AssignedParticipantKind { get; init; }
  public string? AssignedParticipantName { get; init; }
}
