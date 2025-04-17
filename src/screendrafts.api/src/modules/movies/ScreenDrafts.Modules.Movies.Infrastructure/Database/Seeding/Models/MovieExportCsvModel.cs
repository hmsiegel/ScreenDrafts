namespace ScreenDrafts.Modules.Movies.Infrastructure.Database.Seeding.Models;

internal sealed class MovieExportCsvModel
{
  [Column("id")]
  public string Id { get; set; } = default!;

  [Column("imdb_id")]
  public string ImdbId { get; set; } = default!;

  [Column("title")]
  public string Title { get; set; } = default!;

  [Column("year")]
  public string Year { get; set; } = default!;

  [Column("plot")]
  public string Plot { get; set; } = default!;

  [Column("image")]
  public string Image { get; set; } = default!;

  [Column("release_date")]
  public string ReleaseDate { get; set; } = default!;

  [Column("youtube_trailer_url")]
  public string YouTubeTrailerUrl { get; set; } = default!;
}
