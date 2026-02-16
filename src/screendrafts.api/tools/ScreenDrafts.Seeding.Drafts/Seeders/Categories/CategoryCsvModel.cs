namespace ScreenDrafts.Seeding.Drafts.Seeders.Categories;

internal sealed class CategoryCsvModel
{
  [Column("id")]
  public Guid? Id { get; set; }

  [Column("name")]
  public string Name { get; set; } = string.Empty;

  [Column("description")]
  public string Description { get; set; } = string.Empty;
}
