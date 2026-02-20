using ScreenDrafts.Modules.Drafts.Domain.Participants;

namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class DraftPartParticipant : Entity<DraftPartParticipantId>
{
  private readonly List<Pick> _picks = [];
  private readonly List<Veto> _vetoes = [];
  private readonly List<VetoOverride> _vetoOverrides = [];

  private DraftPartParticipant(
    DraftPart draftPart,
    Participant participantId,
    DraftPartParticipantId? id = null)
    : base(id ?? DraftPartParticipantId.CreateUnique())
  {
    DraftPartId = draftPart.Id;
    DraftPart = draftPart;
    ParticipantIdValue = participantId.Value;
    ParticipantKindValue = participantId.Kind;
  }

  private DraftPartParticipant()
  {
    // EF Core
  }

  public DraftPartId DraftPartId { get; private set; } = default!;
  public DraftPart DraftPart { get; private set; } = default!;

  public Guid ParticipantIdValue { get; private set; }
  public ParticipantKind ParticipantKindValue { get; private set; } = default!;

  public Participant ParticipantId => new(ParticipantIdValue, ParticipantKindValue);

  public int StartingVetoes { get; private set; } = 1;

  public int RolloverVeto { get; private set; }
  public int RolloverVetoOverride { get; private set; }

  public int TriviaVetoes { get; private set; }
  public int TriviaVetoOverrides { get; private set; }

  public int TotalVetoes => StartingVetoes + RolloverVeto + TriviaVetoes;
  public int TotalVetoOverrides => RolloverVetoOverride + TriviaVetoOverrides;

  public int CommissionerOverrides { get; private set; }

  public int VetoesUsed { get; private set; }
  public int VetoOverridesUsed { get; private set; }

  public int VetoesRollingOver => TotalVetoes - VetoesUsed >= 1 ? 1 : 0;
  public int VetoOverridesRollingOver => TotalVetoOverrides - VetoOverridesUsed >= 1 ? 1 : 0;

  public bool CanUseVeto() => (TotalVetoes - VetoesUsed) >= 1;
  public bool CanUseVetoOverride(int maxOverrides) => (TotalVetoOverrides - VetoOverridesUsed) >= 1 && maxOverrides > 0;

  public IReadOnlyCollection<Pick> Picks => _picks;
  public IReadOnlyCollection<Veto> Vetoes => _vetoes;
  public IReadOnlyCollection<VetoOverride> VetoOverrides => _vetoOverrides;


  public static DraftPartParticipant Create(
    DraftPart draftPart,
    Participant participantId)
  {
    ArgumentNullException.ThrowIfNull(draftPart);

    return new DraftPartParticipant(
      draftPart,
      participantId);
  }

  internal void SeedSetState(
    int startingVetoes,
    int rolloverVeto,
    int rolloverVetoOverride,
    int triviaVetoes,
    int triviaVetoOverrides,
    int commissionerOverrides,
    int vetoesUsed,
    int vetoOverridesUsed)
  {
     StartingVetoes = startingVetoes;

    RolloverVeto = rolloverVeto;
    RolloverVetoOverride = rolloverVetoOverride;

    TriviaVetoes = triviaVetoes;
    TriviaVetoOverrides = triviaVetoOverrides;

    CommissionerOverrides = commissionerOverrides;

    VetoesUsed = vetoesUsed;
    VetoOverridesUsed = vetoOverridesUsed;
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

  public void SpendVeto()
  {
    if (!CanUseVeto())
    {
      throw new InvalidOperationException("No remaining vetoes.");
    }

    VetoesUsed++;
  }

  public void SpendVetoOverride(int maxOverrides)
  {
    if (!CanUseVetoOverride(maxOverrides))
    {
      throw new InvalidOperationException("No remaining veto overrides.");
    }
    VetoOverridesUsed++;
  }


  internal void AddPick(Pick pick) => _picks.Add(pick);
  internal void AddVeto(Veto veto) => _vetoes.Add(veto);
  internal void AddVetoOverride(VetoOverride vetoOverride) => _vetoOverrides.Add(vetoOverride);
}
