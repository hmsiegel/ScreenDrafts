namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ClearDraftPositionAssignment;

internal sealed record ClearDraftPositionAssignmentRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "positionId")]
  public string PositionId { get; init; } = default!;
}
