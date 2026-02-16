namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ReadModels;

public sealed record ParticipantDraftStatsReadModel
{
  public Guid DraftPartId { get; init; }
  public Guid ParticipantIdValue { get; init; }
  public ParticipantKind ParticipantKind { get; init; } = default!;
  public int StartingVetoes { get; private set; } = 1;
  public int RolloverVeto { get; init; }
  public int RolloverVetoOverride { get; init; }
  public int TriviaVeto { get; init; }
  public int TriviaVetoOverride { get; init; }
  public int VetoesUsed { get; init; }
  public int VetoOverridesUsed { get; init; }
  public int CommissionerOverrides { get; init; }
  public int TotalVetoes => StartingVetoes + RolloverVeto + TriviaVeto;
  public int TotalVetoOverrides => RolloverVetoOverride + TriviaVetoOverride;
  public int VetoesRollingOver => TotalVetoes - VetoesUsed >= 1 ? 1 : 0;
  public int VetoOverridesRollingOver => TotalVetoOverrides - VetoOverridesUsed >=1 ? 1 : 0;

  public int PicksMade { get; init; }
  public int TimesVetoed { get; init; }
  public int VetoesIssued { get; init; }
  public int VetoOverridesIssued { get; init; }
}
