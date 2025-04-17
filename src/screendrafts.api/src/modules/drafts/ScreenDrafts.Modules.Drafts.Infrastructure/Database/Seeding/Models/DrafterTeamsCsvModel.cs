namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database.Seeding.Models;

internal sealed class DrafterTeamsCsvModel
{
  [Column("name")]
  public string Name { get; set; } = default!;
}
