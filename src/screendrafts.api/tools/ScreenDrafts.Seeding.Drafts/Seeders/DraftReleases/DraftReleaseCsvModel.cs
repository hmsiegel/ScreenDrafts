namespace ScreenDrafts.Seeding.Drafts.Seeders.DraftReleases;

internal sealed class DraftReleaseCsvModel
{
  public Guid PartId { get; set; }
  public int ReleaseChannelId { get; set; }
  public int? EpisodeNumber { get; set; }
  public string ReleaseDate { get; set; } = string.Empty;
  public Guid SeriesId { get; set; }
}
