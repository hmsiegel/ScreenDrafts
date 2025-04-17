namespace ScreenDrafts.Modules.Movies.Infrastructure.Database.Seeding.Models;

internal sealed class MovieCsvModel
{
  [Column("title")]
  public string Title { get; set; } = default!;

  [Column("imdb_id")]
  public string ImdbId { get; set; } = default!;
}
