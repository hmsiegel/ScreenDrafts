namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ListDraftPositions;

public sealed record DraftPositionAssignmentResponse
{
  public Guid ParticipantId { get; init; }
  public int ParticipantKind { get; init; } = default!;
  public string? ParticipantName { get; init; }
}
