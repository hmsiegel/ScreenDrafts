namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

public sealed class HostsCsvModel
{
  [Column("id")]
  public Guid? Id { get; set; }

  [Column("person_id")]
  public Guid PersonId { get; set; }
}
