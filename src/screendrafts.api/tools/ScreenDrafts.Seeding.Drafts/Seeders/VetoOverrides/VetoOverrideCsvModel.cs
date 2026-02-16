namespace ScreenDrafts.Seeding.Drafts.Seeders.VetoOverrides;

internal sealed class VetoOverrideCsvModel
{
  [Column("draft_part_id")]
  public Guid DraftPartId { get; set; }

  [Column("play_order")]
  public int PlayOrder { get; set; }

  [Column("veto_issued_by_participant_id")]
  public Guid VetoIssuedByParticipantIdValue { get; set; }

  [Column("veto_issued_by_participant_kind")]
  public int VetoIssuedByParticipantKindValue { get; set; }

  [Column("override_issued_by_participant_id")]
  public Guid OverrideIssuedByParticipantIdValue { get; set; }

  [Column("override_issued_by_participant_kind")]
  public int OverrideIssuedByParticipantKindValue { get; set; }
}
