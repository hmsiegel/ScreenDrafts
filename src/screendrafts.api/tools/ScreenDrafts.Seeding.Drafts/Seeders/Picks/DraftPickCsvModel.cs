namespace ScreenDrafts.Seeding.Drafts.Seeders.Picks;

internal sealed class DraftPickCsvModel
{
  [Column("draft_part_id")]
  public Guid DraftPartId { get; set; }

  [Column("played_by_participant_id_value")]
  public Guid PlayedByParticipantIdValue { get; set; }

  [Column("played_by_participant_kind_value")]
  public int PlayedByParticipantKindValue { get; set; }

  [Column("movie_id")]
  public Guid MovieId { get; set; }

  [Column("position")]
  public int Position { get; set; }

  [Column("play_order")]
  public int PlayOrder { get; set; }

  [Column("movie_version_name")]
  public string? MovieVersionName { get; set; }
}

