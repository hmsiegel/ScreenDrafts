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

  public int RolloverVeto { get; private set; }
  public int? RolloverVetoOverride { get; private set; }

  public int TriviaVetoes { get; private set; }
  public int? TriviaVetoOverrides { get; private set; }

  public int TotalVetoes => StartingVetoes + RolloverVeto + TriviaVetoes;
  public int TotalVetoOverrides => (RolloverVetoOverride ?? 0) + (TriviaVetoOverrides ?? 0);

  public int CommissionerOverrides { get; private set; }

  public int VetoesUsed { get; private set; }
  public int? VetoOverridesUsed { get; private set; }

  public int VetoesRollingOver => TotalVetoes - VetoesUsed >= 1 ? 1 : 0;
  public int VetoOverridesRollingOver => TotalVetoOverrides - VetoOverridesUsed >=1 ? 1 : 0;


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
      RolloverVeto++;
    }
    else
    {
      RolloverVetoOverride++;
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

  public void SetUsedBlessing(int numberOfBlessingsUsed, bool isVeto)
  {
    if (isVeto)
    {
      VetoesUsed += numberOfBlessingsUsed;
    }
    else
    {
      VetoOverridesUsed += numberOfBlessingsUsed;
    }
  }

  public void SetStartingVetoes(int startingVetoes)
  {
    if (startingVetoes < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(startingVetoes), "Starting vetoes cannot be negative.");
    }
    StartingVetoes = startingVetoes;
  }
}
