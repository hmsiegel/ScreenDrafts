namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ClearDraftPositionAssignment;

internal sealed record ClearDraftPositionAssignmentRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }

  [FromRoute(Name = "positionId")]
  public required string PositionPublicId { get; init; }
}
