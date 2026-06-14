namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.TriviaResults.AssignTriviaResults;

internal sealed record AssignTriviaResultsRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;
  public IEnumerable<TriviaResultRequestItem> Results { get; init; } = default!;
}
