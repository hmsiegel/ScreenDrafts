namespace ScreenDrafts.Seeding.Drafts.Seeders.Campaigns;

internal sealed class CampaignCsvModel
{
  [Column("id")]
  public Guid? Id { get; set; }

  [Column("slud")]
  public string Slug { get; set; } = string.Empty;

  [Column("name")]
  public string Name { get; set; } = string.Empty;
}
