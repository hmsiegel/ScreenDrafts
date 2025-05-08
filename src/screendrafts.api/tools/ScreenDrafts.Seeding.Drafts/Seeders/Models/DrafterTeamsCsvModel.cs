namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

internal sealed class DrafterTeamsCsvModel
{
  [Column("name")]
  public string Name { get; set; } = default!;
}
