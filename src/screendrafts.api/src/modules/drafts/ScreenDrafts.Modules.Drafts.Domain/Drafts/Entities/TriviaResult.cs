namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class TriviaResult : Entity<TriviaResultId>
{
  private TriviaResult(
    int position,
    int questionsWon,
    Draft draft,
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    TriviaResultId? id = null)
  {
    Id = id ?? TriviaResultId.CreateUnique();
    Position = position;
    QuestionsWon = questionsWon;

    Draft = draft;
    DraftId = draft.Id;

    Drafter = drafter;
    DrafterId = drafter?.Id;

    DrafterTeam = drafterTeam;
    DrafterTeamId = drafterTeam?.Id;
  }

  private TriviaResult()
  {
  }

  public DraftId DraftId { get; private set; } = default!;
  public Draft Draft { get; private set; } = default!;

  public Drafter? Drafter { get; private set; } = default!;
  public DrafterId? DrafterId { get; private set; } = default!;

  public DrafterTeam? DrafterTeam { get; private set; } = default!;
  public DrafterTeamId? DrafterTeamId { get; private set; } = default!;

  public int Position { get; private set; }

  public int QuestionsWon { get; private set; }

  public static Result<TriviaResult> Create(
    int position,
    int questionsWon,
    Draft draft,
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    TriviaResultId? id = null)
  {
    ArgumentNullException.ThrowIfNull(draft);

    var triviaResult = new TriviaResult(
      position: position,
      questionsWon: questionsWon,
      drafter: drafter,
      draft: draft,
      drafterTeam: drafterTeam,
      id: id);

    return triviaResult;
  }
}
