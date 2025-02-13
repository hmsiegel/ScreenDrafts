namespace ScreenDrafts.Modules.Drafts.Infrastructure.Hosts;

public sealed class HostsCsvModel
{
  [Column("id")]
  public Guid Id { get; set; }

  [Column("name")]
  public string Name { get; set; } = default!;
}
