namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class TriviaResult : Entity<TriviaResultId>
{
  private TriviaResult(
    int position,
    int questionsWon,
    Draft draft,
    Drafter drafter,
    TriviaResultId? id = null)
  {
    Id = id ?? TriviaResultId.CreateUnique();
    Position = position;
    Draft = draft;
    Drafter = drafter;
    QuestionsWon = questionsWon;
  }

  private TriviaResult()
  {
  }

  public DraftId DraftId { get; private set; } = default!;

  public Draft Draft { get; private set; } = default!;

  public Drafter Drafter { get; private set; } = default!;

  public DrafterId DrafterId { get; private set; } = default!;

  public int Position { get; private set; }

  public int QuestionsWon { get; private set; }

  public static Result<TriviaResult> Create(
    int position,
    int questionsWon,
    Draft draft,
    Drafter drafter,
    TriviaResultId? id = null)
  {
    var triviaResult = new TriviaResult(
      position: position,
      questionsWon: questionsWon,
      drafter: drafter,
      draft: draft,
      id: id);
    return triviaResult;
  }
}
