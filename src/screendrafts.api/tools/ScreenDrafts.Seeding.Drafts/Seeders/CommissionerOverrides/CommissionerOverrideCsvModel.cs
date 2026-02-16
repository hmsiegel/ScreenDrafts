namespace ScreenDrafts.Seeding.Drafts.Seeders.CommissionerOverrides;

internal sealed class CommissionerOverrideCsvModel
{
  [Column("draft_part_id")]
  public Guid DraftPartId { get; set; }

  [Column("play_order")]
  public int PlayOrder { get; set; }
}
