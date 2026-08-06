namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AssignSubDraftPosition;

internal sealed record AssignSubDraftPositionCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required string SubDraftId { get; init; }
  public required string WinnerParticipantPublicId { get; init; }
  public required ParticipantKind WinnerParticipantKind { get; init; }
  public required string Choice { get; init; }
}
