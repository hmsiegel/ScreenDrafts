namespace ScreenDrafts.Seeding.Drafts.Seeders.Drafts;

internal sealed class DraftsModel
{
  [JsonPropertyName("id")]
  public Guid? Id { get; set; }

  [JsonPropertyName("title")]
  public string Title { get; set; } = string.Empty;

  [JsonPropertyName("draft_type")]
  public int DraftType { get; set; }

  [JsonPropertyName("draft_status")]
  public int DraftStatus { get; set; } = Modules.Drafts.Domain.Drafts.Enums.DraftStatus.Completed;

  [JsonPropertyName("series_id")]
  public Guid SeriesId { get; set; }

  [JsonPropertyName("campaign_id")]
  public Guid? CampaignId { get; set; }
}
