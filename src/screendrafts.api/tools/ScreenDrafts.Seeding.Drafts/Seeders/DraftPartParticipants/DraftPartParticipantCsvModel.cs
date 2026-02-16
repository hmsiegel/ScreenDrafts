namespace ScreenDrafts.Seeding.Drafts.Seeders.DraftPartParticipants;

internal sealed class DraftPartParticipantCsvModel
{
  [Column("draft_part_id")]
  public Guid DraftPartId { get; set; }

  [Column("participant_id")]
  public Guid ParticipantId { get; set; }

  [Column("participant_kind")]
  public int ParticipantKind { get; set; }

  [Column("starting_vetoes")]
  public int StartingVetoes { get; set; }

  [Column("rollover_veto")]
  public int RolloverVeto { get; set; }

  [Column("rollover_veto_override")]
  public int RolloverVetoOverride { get; set; }

  [Column("trivia_vetoes")]
  public int TriviaVetoes { get; set; }

  [Column("trivia_veto_overrides")]
  public int TriviaVetoOverrides { get; set; }

  [Column("commissioner_overrides")]
  public int CommissionerOverrides { get; set; }

  [Column("vetoes_used")]
  public int VetoesUsed { get; set; }

  [Column("veto_overrides_used")]
  public int VetoOverridesUsed { get; set; }
}
