namespace ScreenDrafts.Seeding.Drafts.Seeders.DraftPositions;

internal sealed class DraftPositionCsvModel
{
  [Column("draft_part_id")]
  public Guid DraftPartId { get; set; }

  [Column("name")]
  public string PositionName { get; set; } = string.Empty;

  [Column("picks")]
  public string DraftPicks { get; set; } = string.Empty;

  [Column("has_bonus_veto")]
  public bool? HasBonusVeto { get; set; } = false;

  [Column("has_bonus_veto_override")]
  public bool? HasBonusVetoOverride { get; set; } = false;

  [Column("assigned_to_id")]
  public Guid? AssignedToId { get; set; } = null;

  [Column("assigned_to_kind")]
  public int? AssignedToKind { get; set; } = null;
}
