namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class TriviaResult : Entity<TriviaResultId>
{
  public TriviaResult(
    Ulid draftId,
    Ulid drafterId,
    bool awardIsVetoed,
    int positeon,
    TriviaResultId? id = null)
  {
    Id = id ?? TriviaResultId.CreateUnique();
    DraftId = draftId;
    DrafterId = drafterId;
    AwardIsVetoed = awardIsVetoed;
    Position = positeon;
  }

  private TriviaResult()
  {
  }

  public Ulid DraftId { get; private set; }
  public Ulid DrafterId { get; private set; }
  public bool AwardIsVetoed { get; private set; } // True if veto, false if veto override
  public int Position { get; private set; }
}
