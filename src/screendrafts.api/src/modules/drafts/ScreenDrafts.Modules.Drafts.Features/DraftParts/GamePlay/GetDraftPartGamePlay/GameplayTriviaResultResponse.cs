namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

internal sealed record GameplayTriviaResultResponse
{
  public Guid ParticipantId { get; init; }
  public int ParticipantKind { get; init; }
  public string ParticipantName { get; init; } = default!;
  public int QuestionsWon { get; init; }
  public int Position { get; init; }
}
