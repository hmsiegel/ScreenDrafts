namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AssignSubDraftTriviaResults;

internal sealed record AssignSubDraftTriviaRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartPublicId { get; init; }

  [FromRoute(Name = "subDraftId")]
  public required string SubDraftPublicId { get; init; }

  public IEnumerable<TriviaResultRequestItem> Results { get; init; } = default!;
}
