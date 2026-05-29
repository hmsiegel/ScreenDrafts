namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.TriviaResults.GetTriviaResults;

internal sealed record GetTriviaResultsRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }
}
