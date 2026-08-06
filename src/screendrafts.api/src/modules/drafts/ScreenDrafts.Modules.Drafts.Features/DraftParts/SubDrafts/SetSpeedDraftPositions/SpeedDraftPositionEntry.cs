namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.SetSpeedDraftPositions;

internal sealed record SpeedDraftPositionEntry
{
  public required string Name { get; init; } // "A" or "B"
  public required IReadOnlyList<int> Picks { get; init; }
}
