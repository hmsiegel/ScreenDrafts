namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.AssignParticipantToDraftPosition;

internal sealed record AssignParticipantToDraftPositionCommand : ICommand
{
  public required string DraftPartId { get; init; }

  public required string PositionPublicId { get; init; }

  public string? ParticipantPublicId { get; init; }
  public required ParticipantKind ParticipantKind { get; init; }
}
