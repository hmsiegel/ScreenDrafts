namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class DrafterDraftStats : Entity<DrafterDraftStatsId>
{
  private DrafterDraftStats(
    Drafter drafter,
    Draft draft,
    DrafterDraftStatsId? id = null)
  {
    Id = id ?? DrafterDraftStatsId.CreateUnique();
    Drafter = drafter;
    Draft = draft;
  }

  private DrafterDraftStats()
  {
  }

  public DrafterId DrafterId { get; private set; } = default!;

  public Drafter Drafter { get; private set; } = default!;

  public DraftId DraftId { get; private set; } = default!;

  public Draft Draft { get; private set; } = default!;


  public int StartingVetoes { get; private set; } = 1;

  public int StartingVetoOverrides { get; private set; }

  public int RolloversApplied { get; private set; }

  public int TriviaVetoes { get; private set; }

  public int TriviaVetoOverrides { get; private set; }

  public int TotalVetoes => StartingVetoes + RolloversApplied + TriviaVetoes;

  public int TotalVetoOverrides => StartingVetoOverrides + TriviaVetoOverrides;

  public static DrafterDraftStats Create(Drafter drafter, Draft draft) => new(drafter, draft);

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
