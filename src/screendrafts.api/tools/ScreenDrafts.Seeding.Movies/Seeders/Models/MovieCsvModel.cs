namespace ScreenDrafts.Seeding.Movies.Seeders.Models;

internal sealed class MovieCsvModel
{
  [Column("title")]
  public string Title { get; set; } = default!;

  [Column("tmdb_id")]
  public int TmdbId { get; set; } = default!;
}
