using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafters;

public sealed class DrafterCsvModel
{
  [Column("id")]
  public Guid Id { get; set; }

  [Column("name")]
  public string Name { get; set; } = default!;
}
