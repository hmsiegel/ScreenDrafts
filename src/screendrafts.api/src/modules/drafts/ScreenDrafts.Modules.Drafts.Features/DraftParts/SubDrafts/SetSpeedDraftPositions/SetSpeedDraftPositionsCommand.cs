namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.SetSpeedDraftPositions;

internal sealed record SetSpeedDraftPositionsCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required IReadOnlyList<SpeedDraftPositionEntry> Positions { get; init; }
}
