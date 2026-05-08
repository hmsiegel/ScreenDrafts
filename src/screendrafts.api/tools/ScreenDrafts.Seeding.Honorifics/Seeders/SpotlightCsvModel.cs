namespace ScreenDrafts.Seeding.Honorifics.Seeders;

internal sealed class SpotlightCsvModel
{
  [Column("draft_public_id")]
  public string DraftPublicId { get; set; } = string.Empty;

  [Column("spotlight_description")]
  public string SpotlightDescription { get; set; } = string.Empty;

  [Column("spotify_url")]
  public string? SpotifyUrl { get; set; }
}
