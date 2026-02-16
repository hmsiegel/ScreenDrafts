namespace ScreenDrafts.Seeding.Drafts.Seeders.DraftParts;

internal sealed class DraftPartCsvModel
{
  public Guid? Id { get; set; }
  public Guid DraftId { get; set; }
  public int PartIndex { get; set; }
  public int Min { get; set; }
  public int Max { get; set; }
}
