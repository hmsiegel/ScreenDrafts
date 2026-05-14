namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.AssignParticipantToDraftPosition;

internal sealed record AssignParticipantToDraftPositionRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "positionId")]
  public string PositionId { get; init; } = default!;

  public string? ParticipantPublicId { get; init; }
  public required int ParticipantKind { get; init; }
}
