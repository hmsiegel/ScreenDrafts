namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.SetDraftPosition;

internal sealed record SetDraftPositionsRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }
  public IReadOnlyList<DraftPositionRequestModel> Positions { get; init; } = [];
}
