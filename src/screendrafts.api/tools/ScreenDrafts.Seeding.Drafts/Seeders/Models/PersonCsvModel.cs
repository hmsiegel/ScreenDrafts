namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

internal sealed class PersonCsvModel
{
  [Column("id")]
  public Guid Id { get; set; }

  [Column("first_name")]
  public string FirstName { get; set; } = default!;

  [Column("last_name")]
  public string LastName { get; set; } = default!;

  [Column("display_name")]
  public string DisplayName { get; set; } = default!;
}
