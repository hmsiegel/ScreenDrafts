namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.SetDraftPosition;

internal sealed record SetDraftPositionsRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;
  public IReadOnlyList<DraftPositionRequestModel> Positions { get; init; } = [];
}
