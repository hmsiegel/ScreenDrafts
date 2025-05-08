namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

public sealed class MoviesCsvModel
{
  [Column("id")]
  public Guid Id { get; set; }

  [Column("movie_title")]
  public string Title { get; set; } = default!;

  [Column("imdb_id")]
  public string ImdbId { get; set; } = default!;
}
