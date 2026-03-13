namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ClearDraftPositionAssignment;

internal sealed record ClearDraftPositionAssignmentCommand : ICommand
{
  public string DraftPartId { get; init; } = default!;

  public string PositionPublicId { get; init; } = default!;
}
