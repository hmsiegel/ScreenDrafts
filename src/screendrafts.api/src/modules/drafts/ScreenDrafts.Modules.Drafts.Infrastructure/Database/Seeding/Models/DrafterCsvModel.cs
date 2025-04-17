namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database.Seeding.Models;

public sealed class DrafterCsvModel
{
  [Column("id")]
  public Guid Id { get; set; }

  [Column("name")]
  public string Name { get; set; } = default!;
}
