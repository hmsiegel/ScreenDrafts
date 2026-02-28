namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AssignTriviaResults;

internal sealed record AssignTriviaResultsCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public IEnumerable<TriviaResultEntry> Results { get; init; } = default!;
}

