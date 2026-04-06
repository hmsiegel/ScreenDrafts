namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AssignSubDraftTriviaResults;

internal sealed record AssignSubDraftTriviaCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public required string SubDraftPublicId { get; init; }
  public IEnumerable<TriviaResultEntry> Results { get; init; } = default!;
}
