namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.AssignParticipantToDraftPosition;

internal sealed record AssignParticipantToDraftPositionRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }

  [FromRoute(Name = "positionId")]
  public required string PositionPublicId { get; init; }

  public string? ParticipantPublicId { get; init; }
  public required int ParticipantKind { get; init; }
}
