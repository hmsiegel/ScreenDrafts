namespace ScreenDrafts.Seeding.Movies.Seeders.Models;

internal sealed class ProductionCompanyExportCsvModel
{
  [Column("id")]
  public string Id { get; set; } = default!;

  [Column("name")]
  public string Name { get; set; } = default!;

  [Column("imdb_id")]
  public string ImdbId { get; set; } = default!;
}
