namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

internal sealed class DrafterCsvModel
{
  [Column("id")]
  public Guid? Id { get; set; }

  [Column("person_id")]
  public Guid PersonId { get; set; }
}
