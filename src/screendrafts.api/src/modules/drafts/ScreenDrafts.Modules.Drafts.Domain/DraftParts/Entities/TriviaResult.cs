using System.Data;

namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class TriviaResult : Entity<TriviaResultId>
{
  private TriviaResult(
    int position,
    int questionsWon,
    Participant participantId,
    DraftPart draftPart,
    TriviaResultId? id = null)
  {
    Id = id ?? TriviaResultId.CreateUnique();
    Position = position;
    QuestionsWon = questionsWon;
    ParticipantId = participantId;
    DraftPart = draftPart;
    DraftPartId = draftPart.Id;
  }

  private TriviaResult()
  {
  }

  public DraftPartId DraftPartId { get; private set; } = default!;
  public DraftPart DraftPart { get; private set; } = default!;
  public SubDraftId? SubDraftId { get; private set; } = default!;

  public Participant ParticipantId { get; private set; } = default!;


  public int Position { get; private set; }

  public int QuestionsWon { get; private set; }

  public static Result<TriviaResult> Create(
    int position,
    int questionsWon,
    Participant participantId,
    DraftPart draftPart,
    TriviaResultId? id = null)
  {
    ArgumentNullException.ThrowIfNull(draftPart);

    if (position <= 0)
    {
      return Result.Failure<TriviaResult>(TriviaResultErrors.TriviaResultPositionInvalid);
    }

    if (questionsWon < 0)
    {
      return Result.Failure<TriviaResult>(TriviaResultErrors.TriviaResultQuestionsWonInvalid);
    }

    var triviaResult = new TriviaResult(
      position: position,
      questionsWon: questionsWon,
      participantId: participantId,
      draftPart: draftPart,
      id: id);

    return triviaResult;
  }

  public static Result<TriviaResult> CreateForSubDraft(
    int position,
    int questionsWon,
    Participant participantId,
    DraftPart draftPart,
    SubDraftId subDraftId,
    TriviaResultId? id = null)
  {
    ArgumentNullException.ThrowIfNull(draftPart);

    if (position <= 0)
    {
      return Result.Failure<TriviaResult>(TriviaResultErrors.TriviaResultPositionInvalid);
    }

    if (questionsWon < 0)
    {
      return Result.Failure<TriviaResult>(TriviaResultErrors.TriviaResultQuestionsWonInvalid);
    }

    var result = Create(
      position: position,
      questionsWon: questionsWon,
      participantId: participantId,
      draftPart: draftPart,
      id: id);

    if (result.IsFailure)
    {
      return result;
    }

    var triviaResult = result.Value;
    triviaResult.SubDraftId = subDraftId;

    return triviaResult;
  }
}
