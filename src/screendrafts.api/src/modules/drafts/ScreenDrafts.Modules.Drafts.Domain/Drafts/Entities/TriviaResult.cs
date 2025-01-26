namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class TriviaResult : Entity<TriviaResultId>
{
  private TriviaResult(
    int positeon,
    Drafter drafter,
    BlessingType blessingType,
    TriviaResultId? id = null)
  {
    Id = id ?? TriviaResultId.CreateUnique();
    Position = positeon;
    BlessingType = blessingType;
    Drafter = drafter;
  }

  private TriviaResult()
  {
  }

  public Drafter Drafter { get; private set; } = default!;
  public BlessingType BlessingType { get; private set; } = default!;
  public int Position { get; private set; }

  public static Result<TriviaResult> Create(
    int position,
    Drafter drafter,
    BlessingType blessingType,
    TriviaResultId? id = null)
  {
    var triviaResult = new TriviaResult(
      positeon: position,
      drafter: drafter,
      blessingType: blessingType,
      id: id);
    return triviaResult;
  }
}
