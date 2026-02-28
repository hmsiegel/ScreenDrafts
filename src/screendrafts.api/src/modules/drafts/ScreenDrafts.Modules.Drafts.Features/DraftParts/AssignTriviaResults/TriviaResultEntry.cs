namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AssignTriviaResults;

internal sealed record TriviaResultEntry
{
  public required string ParticipantPublicId { get; init; }
  public required ParticipantKind Kind { get; init; }
  public required int Position { get; init; }
  public required int QuestionsWon { get; init; }
}

