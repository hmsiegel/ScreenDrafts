namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

internal sealed class DrafterCsvModel
{
  [Column("id")]
  public Guid Id { get; set; }

  [Column("name")]
  public string Name { get; set; } = default!;
}
