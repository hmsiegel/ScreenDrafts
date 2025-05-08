namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

internal sealed class VetoOverrideCsvModel
{
  [Column("veto_id")]
  public Guid? VetoId { get; set; }
  [Column("drafter_id")]
  public Guid? DrafterId { get; set; } // Drafter who vetoed the pick
  [Column("drafter_team_id")]
  public Guid? DrafterTeamId { get; set; } // Drafter team who vetoed the pick

  public Guid? OverrideByDrafterId { get; set; } // Drafter who is overriding the veto
  public Guid? OverrideByDrafterTeamId { get; set; } // Drafter team who is overriding the veto

  public Guid DraftId { get; set; }
  public int Position { get; set; }
  public Guid MovieId { get; set; }
}
