namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AssignTriviaResults;

internal sealed record AssignTriviaResultsRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartPublicId { get; init; }
  public IEnumerable<TriviaResultRequestItem> Results { get; init; } = default!;
}

