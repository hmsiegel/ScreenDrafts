namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

internal sealed class DraftStatsCsvModel
{
  [Column("drafter_id")]
  public Guid? DrafterId { get; set; }

  [Column("drafter_team_id")]
  public Guid? DrafterTeamId { get; set; }

  [Column("draft_id")]
  public Guid DraftId { get; set; }

  [Column("starting_vetoes")]
  public int StartingVetoes { get; set; } = 1;

  [Column("rollover_vetoes")]
  public int RolloverVetoes { get; set; } = 0;

  [Column("rollover_veto_overrides")]
  public int? RolloverVetoOverrides { get; set; } = 0;

  [Column("commissioner_overrides")]
  public int CommissionerOverrides { get; set; } = 0;

  [Column("trivia_vetoes")]
  public int TriviaVetoes { get; set; } = 0;

  [Column("trivia_veto_overrides")]
  public int? TriviaVetoOverrides { get; set; } = 0;

  [Column("vetoes_used")]
  public int VetoesUsed { get; set; } = 0;

  [Column("veto_overrides_used")]
  public int? VetoOverridesUsed { get; set; } = 0;
}
