namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

internal sealed class VetoCsvModel
{
  [Column("pick_id")]
  public Guid? PickId { get; set; }
  [Column("drafter_id")]
  public Guid? DrafterId { get; set; }
  [Column("drafter_team_id")]
  public Guid? DrafterTeamId { get; set; }


  public Guid? DraftId { get; set; }
  public int? Position { get; set; }
  public Guid? MovieId { get; set; }
}
