namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.TriviaResults.GetTriviaResults;

internal sealed record GetTriviaResultsResponse
{
  public required string DraftPartId { get; init; }
  public required IReadOnlyList<TriviaResultResponse> Results { get; init; } = [];
}
