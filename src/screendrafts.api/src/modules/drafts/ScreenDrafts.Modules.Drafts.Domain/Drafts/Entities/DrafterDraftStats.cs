namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class DrafterDraftStats : Entity<DrafterDraftStatsId>
{
  private DrafterDraftStats(
    Guid drafterId,
    Guid draftId,
    DrafterDraftStatsId? id = null)
  {
    Id = id ?? DrafterDraftStatsId.CreateUnique();
    DrafterId = drafterId;
    DraftId = draftId;
  }

  private DrafterDraftStats()
  {
  }

  public Guid DrafterId { get; private set; }

  public Guid DraftId { get; private set; }


  public int StartingVetoes { get; private set; } = 1;

  public int StartingVetoOverrides { get; private set; }

  public int RolloversApplied { get; private set; }

  public int TriviaVetoes { get; private set; }

  public int TriviaVetoOverrides { get; private set; }

  public int TotalVetoes => StartingVetoes + RolloversApplied + TriviaVetoes;

  public int TotalVetoOverrides => StartingVetoOverrides + TriviaVetoOverrides;

  public static DrafterDraftStats Create(Guid drafterId, Guid draftId) => new(drafterId, draftId);

  public void AddRollover(bool isVeto)
  {
    if (isVeto)
    {
      RolloversApplied++;
    }
    else
    {
      StartingVetoOverrides++;
    }
  }

  public void AddTriviaAward(bool isVeto)
  {
    if (isVeto)
    {
      TriviaVetoes++;
    }
    else
    {
      TriviaVetoOverrides++;
    }
  }
}
