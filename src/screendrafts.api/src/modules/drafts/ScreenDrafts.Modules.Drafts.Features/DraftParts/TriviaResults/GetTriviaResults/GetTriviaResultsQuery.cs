namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.TriviaResults.GetTriviaResults;

internal sealed record GetTriviaResultsQuery : IQuery<GetTriviaResultsResponse>
{
  public required string DraftPartId { get; init; }
}
