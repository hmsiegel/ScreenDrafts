namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

internal sealed class CommissionerOverrideCsvModel
{
  [Column("pick_id")]
  public Guid? PickId { get; set; }

  public Guid? DraftId { get; set; }
  public int? Position { get; set; }
  public Guid? MovieId { get; set; }

  public Guid? DrafterId { get; set; }
  public Guid? DrafterTeamId { get; set; }
}
