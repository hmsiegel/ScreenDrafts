namespace ScreenDrafts.Seeding.Movies.Seeders.Models;

internal sealed class MovieCsvModel
{
  [Column("title")]
  public string Title { get; set; } = default!;

  [Column("media_type")]
  public int MediaType { get; set; } = 0;

  [Column("imdb_id")]
  public string? ImdbId { get; set; }

  [Column("tmdb_id")]
  public int? TmdbId { get; set; }

  [Column("igdb_id")]
  public int? IgdbId { get; set; }

  [Column("tv_series_tmdb_id")]
  public int? TvSeriesTmdbId { get; set; }

  [Column("season_number")]
  public int? SeasonNumber { get; set; }

  [Column("episode_number")]
  public int? EpisodeNumber { get; set; }
}
