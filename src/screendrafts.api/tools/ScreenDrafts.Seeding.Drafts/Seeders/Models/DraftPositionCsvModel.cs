namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

internal sealed class DraftPositionCsvModel
{
  [Column("game_board_id")]
  public Guid GameBoardId { get; set; }

  [Column("name")]
  public string PositionName { get; set; } = string.Empty;

  [Column("picks")]
  public string DraftPicks { get; set; } = string.Empty;

  [Column("has_bonus_veto")]
  public bool HasBonusVeto { get; set; } = false;

  [Column("has_bonus_veto_override")]
  public bool HasBonusVetoOverride { get; set; } = false;

  [Column("drafter_id")]
  public Guid? DrafterId { get; set; } = null;

  [Column("drafter_team_id")]
  public Guid? DrafterTeamId { get; set; } = null;
}
