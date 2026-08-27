namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.SetPartPositionRange;

internal sealed record SetPartPositionRangeCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public int MinimumPosition { get; init; }
  public int MaximumPosition { get; init; }
}
