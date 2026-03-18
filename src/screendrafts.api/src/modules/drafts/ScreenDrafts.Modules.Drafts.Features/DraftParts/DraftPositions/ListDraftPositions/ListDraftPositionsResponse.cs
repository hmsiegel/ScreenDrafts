namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ListDraftPositions;

internal sealed record ListDraftPositionsResponse
{
  public IReadOnlyCollection<DraftPositionResponse> Positions { get; init; } = [];
}
