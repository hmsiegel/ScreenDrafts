namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.SetPartPositionRange;

internal sealed record SetPartPositionRangeRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;
  public int MinimumPosition { get; init; }
  public int MaximumPosition { get; init; }
}
