namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

internal sealed class DraftPickCsvModel
{
  [Column("draft_id")]
  public Guid DraftId { get; set; }

  [Column("drafter_id")]
  public Guid? DrafterId { get; set; }

  [Column("drafter_team_id")]
  public Guid? DrafterTeamId { get; set; }

  [Column("pick_number")]
  public int PickNumber { get; set; }

  [Column("movie_id")]
  public Guid MovieId { get; set; }

  [Column("play_order")]
  public int PlayOrder { get; set; }
}
