namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class TriviaResult : Entity<TriviaResultId>
{
  private TriviaResult(
    int position,
    int questionsWon,
    DraftPart draftPart,
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    TriviaResultId? id = null)
  {
    Id = id ?? TriviaResultId.CreateUnique();
    Position = position;
    QuestionsWon = questionsWon;

    DraftPart = draftPart;
    DraftPartId = draftPart.Id;

    Drafter = drafter;
    DrafterId = drafter?.Id;

    DrafterTeam = drafterTeam;
    DrafterTeamId = drafterTeam?.Id;
  }

  private TriviaResult()
  {
  }

  public DraftPartId DraftPartId { get; private set; } = default!;
  public DraftPart DraftPart { get; private set; } = default!;

  public Drafter? Drafter { get; private set; } = default!;
  public DrafterId? DrafterId { get; private set; } = default!;

  public DrafterTeam? DrafterTeam { get; private set; } = default!;
  public DrafterTeamId? DrafterTeamId { get; private set; } = default!;

  public int Position { get; private set; }

  public int QuestionsWon { get; private set; }

  public static Result<TriviaResult> Create(
    int position,
    int questionsWon,
    DraftPart draftPart,
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    TriviaResultId? id = null)
  {
    ArgumentNullException.ThrowIfNull(draftPart);

    var triviaResult = new TriviaResult(
      position: position,
      questionsWon: questionsWon,
      drafter: drafter,
      draftPart: draftPart,
      drafterTeam: drafterTeam,
      id: id);

    return triviaResult;
  }
}
