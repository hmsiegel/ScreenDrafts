namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ListDraftPositions;

internal sealed record ListDraftPositionsRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }
}
