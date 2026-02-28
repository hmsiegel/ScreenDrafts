namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AssignTriviaResults;

internal sealed record TriviaResultRequestItem
{
  public required string ParticipantPublicId { get; init; }
  public required int Kind { get; init; }
  public required int Position { get; init; }
  public required int QuestionsWon { get; init; }
}

