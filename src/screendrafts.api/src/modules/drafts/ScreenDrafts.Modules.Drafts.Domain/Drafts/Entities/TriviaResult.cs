namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class TriviaResult : Entity<TriviaResultId>
{
  private TriviaResult(
    Guid draftId,
    Guid drafterId,
    bool awardIsVetoed,
    int positeon,
    TriviaResultId? id = null)
  {
    Id = id ?? TriviaResultId.CreateUnique();
    DraftId = draftId;
    DrafterId = drafterId;
    AwardIsVeto = awardIsVetoed;
    Position = positeon;
  }

  private TriviaResult()
  {
  }

  public Guid DraftId { get; private set; }
  public Guid DrafterId { get; private set; }
  public bool AwardIsVeto { get; private set; } // True if veto, false if veto override
  public int Position { get; private set; }

  public static Result<TriviaResult> Create(
    Guid draftId,
    Guid drafterId,
    bool awardIsVetoed,
    int position,
    TriviaResultId? id = null)
  {
    var triviaResult = new TriviaResult(
      draftId: draftId,
      drafterId: drafterId,
      awardIsVetoed: awardIsVetoed,
      positeon: position,
      id: id);
    return triviaResult;
  }
}
