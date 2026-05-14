namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AssignSubDraftTriviaResults;

internal sealed record AssignSubDraftTriviaRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartPublicId { get; init; } = default!;

  [FromRoute(Name = "subDraftId")]
  public string SubDraftPublicId { get; init; } = default!;

  public IEnumerable<TriviaResultRequestItem> Results { get; init; } = default!;
}
