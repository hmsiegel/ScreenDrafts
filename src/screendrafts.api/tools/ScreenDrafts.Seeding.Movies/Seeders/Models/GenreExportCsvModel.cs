namespace ScreenDrafts.Seeding.Movies.Seeders.Models;

internal sealed class GenreExportCsvModel
{
  [Column("id")]
  public string Id { get; set; } = default!;

  [Column("name")]
  public string Name { get; set; } = default!;
}
