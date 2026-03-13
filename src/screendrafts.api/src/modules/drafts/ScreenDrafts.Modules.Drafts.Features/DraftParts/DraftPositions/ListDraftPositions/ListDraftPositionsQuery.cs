namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ListDraftPositions;

internal sealed record ListDraftPositionsQuery : IQuery<ListDraftPositionsResponse>
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }
}
