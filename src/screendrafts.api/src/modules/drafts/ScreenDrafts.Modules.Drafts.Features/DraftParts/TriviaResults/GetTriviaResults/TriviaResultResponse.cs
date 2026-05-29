namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.TriviaResults.GetTriviaResults;

internal sealed record TriviaResultResponse
{
  public required int Position { get; init; }
  public required int QuestionsWon { get; init; }
  public required string ParticipantDisplayName { get; init; }
  public required string ParticipantKind { get; init; }

  /// <summary>
  /// Null for standard draft trivia. For Speed Drafts, the 1-based index
  /// of the sub-draft this trivia result belongs to.
  /// </summary>
  public int? SubDraftIndex { get; init; }
}
