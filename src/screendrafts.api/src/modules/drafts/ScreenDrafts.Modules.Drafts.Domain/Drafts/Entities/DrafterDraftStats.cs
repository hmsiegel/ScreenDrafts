namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class DrafterDraftStats : Entity<DrafterDraftStatsId>
{
  private DrafterDraftStats(
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    Draft draft,
    DrafterDraftStatsId? id = null)
  {
    Id = id ?? DrafterDraftStatsId.CreateUnique();

    Drafter = drafter;
    DrafterId = drafter?.Id;

    DrafterTeam = drafterTeam;
    DrafterTeamId = drafterTeam?.Id;

    Draft = draft;
    DraftId = draft.Id;
  }

  private DrafterDraftStats()
  {
  }

  public DraftId DraftId { get; private set; } = default!;
  public Draft Draft { get; private set; } = default!;

  public DrafterId? DrafterId { get; private set; } = default!;
  public Drafter? Drafter { get; private set; } = default!;

  public DrafterTeamId? DrafterTeamId { get; private set; } = default!;
  public DrafterTeam? DrafterTeam { get; private set; } = default!;


  public int StartingVetoes { get; private set; } = 1;

  public int StartingVetoOverrides { get; private set; }

  public int CommissionerOverrides { get; private set; }

  public int RolloversApplied { get; private set; }

  public int TriviaVetoes { get; private set; }

  public int TriviaVetoOverrides { get; private set; }


  public int TotalVetoes => StartingVetoes + RolloversApplied + TriviaVetoes;

  public int TotalVetoOverrides => StartingVetoOverrides + TriviaVetoOverrides;


  public static DrafterDraftStats Create(
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    Draft draft)
  {
    ArgumentNullException.ThrowIfNull(draft);
    var drafterDraftStats = new DrafterDraftStats(drafter, drafterTeam, draft);
    return drafterDraftStats;
  }

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

  public void AddCommissionerOverride()
  {
    CommissionerOverrides++;
  }
}
